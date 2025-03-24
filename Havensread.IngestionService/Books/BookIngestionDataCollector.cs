using Havensread.IngestionService.Apis;
using Microsoft.Extensions.AI;
using Microsoft.ML.Tokenizers;
using Microsoft.SemanticKernel.Text;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Havensread.IngestionService;

public sealed class BookIngestionDataCollector
{
    public sealed record Request(Guid Id, string Title, string? ISBN);

    public sealed class Response
    {
        public required Guid Id { get; init; }
        public required string Title { get; init; }
        public required Status Status { get; init; }
        public BookInformation? Data { get; init; }
        public string? SourceLink { get; init; }
        public int Score { get; set; }

        [MemberNotNullWhen(true, nameof(Data))]
        [MemberNotNullWhen(true, nameof(SourceLink))]
        public bool Success => Status is Status.Complete;
    }

    public sealed record BookInformation
    {
        public string? Synopsis { get; init; }
        public IEnumerable<string>? Genres { get; init; }
        public string? MainAuthorName { get; init; }
        public IEnumerable<string>? MentionedAuthorNames { get; init; }
        public string? AuthorInformation { get; init; }

        [MemberNotNullWhen(false, nameof(Synopsis))]
        [MemberNotNullWhen(false, nameof(Genres))]
        [MemberNotNullWhen(false, nameof(MainAuthorName))]
        public bool Error { get; set; }
    }

    public enum Status : byte
    {
        Unknown,
        Complete,
        BadResult,
        BadQuery,
        ErrorQuery,
    }

    private readonly HttpClient _jinaClient;
    private readonly HttpClient _googleClient;
    private readonly IChatClient _chatClient;
    private readonly ILogger<BookIngestionDataCollector> _logger;
    private readonly ChatOptions _chatOptions;
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    };
    private readonly Tokenizer _tokenizer;

    private const string Prompt = """
        You will be provided with a chunk of text taken from the internet.
        Your mission is to extract text that describes the book {0}.
        We are interested in the following required information:
        - Book synopsis
        - Genres
        - Names of the main author of the book
        - Additional author information (optional)
        - Additional mentioned author names (optional)

        Make sure to get specically those genres that pertain to this book.
        All information should be verbatim from the text.

        If there is any information that is not relevant to the book or our mission, please ignore it.
        If there are any required information missing please set the value of Error to true.

        Bless you and good luck.

        Text:
        {1}
        """;

    public BookIngestionDataCollector(
        IHttpClientFactory httpClientFactory,
        IChatClient chatClient,
        ILogger<BookIngestionDataCollector> logger)
    {
        _jinaClient = httpClientFactory.CreateClient("jina-reader");
        _googleClient = httpClientFactory.CreateClient("book-searcher");
        _chatClient = chatClient;
        _logger = logger;
        _chatOptions = new ChatOptions
        {
            Temperature = 0.5f,
        };
        _tokenizer = TiktokenTokenizer.CreateForModel("gpt-4o-mini");
    }

    // note:
    // i should include book similarities, reviews
    // author notes? or is that perhaps a seperate process
    public async IAsyncEnumerable<Response> ExecuteAsync(
        IEnumerable<Request> requests,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        List<Response> responses = [];

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = 1, //Environment.ProcessorCount,
            CancellationToken = cancellationToken
        };

        await Parallel.ForEachAsync(requests, options, async (request, token) =>
        {
            var url = $"{_googleClient.BaseAddress}&q={Uri.EscapeDataString(request.Title)}";
            var searchResponse = await _googleClient.GetFromJsonAsync<GoogleSearch.Result>(url);
            var searchItem = searchResponse?.Items?.FirstOrDefault();

            if (searchItem is null)
            {
                responses.Add(new Response { Id = request.Id, Title = request.Title, Status = Status.BadQuery });
                return;
            }

            var content = await _jinaClient.GetStringAsync($"{_jinaClient.BaseAddress}{searchItem.Link}");

            var chunks = TextChunker.SplitPlainTextParagraphs([content], 8000, 1000, request.Title, text => _tokenizer.CountTokens(text));

            var prompt = string.Format(Prompt, request.Title, chunks[0]);
            var response = await _chatClient.GetResponseAsync<BookInformation>(prompt, s_jsonSerializerOptions, _chatOptions, true, cancellationToken);

            if (response.Result.Error)
            {
                responses.Add(new Response { Id = request.Id, Title = request.Title, Status = Status.BadResult, SourceLink = searchItem.Link });
                return;
            }

            responses.Add(new Response
            {
                Id = request.Id,
                Status = Status.Complete,
                Data = new()
                {
                    Synopsis = response.Result.Synopsis,
                    Genres = response.Result.Genres,
                    AuthorInformation = response.Result.AuthorInformation
                },
                Title = request.Title,
                SourceLink = searchItem.Link
            });
        });

        foreach (var response in responses)
        {
            yield return response;
        }
    }
}
