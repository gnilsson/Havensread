using Havensread.IngestionService.Apis;
using Havensread.ServiceDefaults.Misc;
using Microsoft.Extensions.AI;
using Microsoft.ML.Tokenizers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Havensread.IngestionService.Workers.Book;

public sealed partial class BookIngestionHandler
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
        Error,
    }

    private readonly HttpClient _jinaClient;
    private readonly HttpClient _googleClient;
    private readonly IChatClient _chatClient;
    private readonly ILogger<BookIngestionHandler> _logger;
    private readonly ChatOptions _chatOptions;
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    };
    private readonly Tokenizer _tokenizer;
    private const int MaxCharacterCount = 14_000;

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

    public BookIngestionHandler(
        IHttpClientFactory httpClientFactory,
        IChatClient chatClient,
        ILogger<BookIngestionHandler> logger)
    {
        _googleClient = httpClientFactory.CreateClient("book-searcher");
        _jinaClient = httpClientFactory.CreateClient("jina-reader");
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
        IAsyncEnumerable<Request> requests,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var request in requests)
        {
            var searchItem = await GetFirstGoogleSearchResultAsync(request);

            if (searchItem is null)
            {
                yield return new() { Id = request.Id, Title = request.Title, Status = Status.BadQuery };
                continue;
            }



            //var content = await _jinaClient.GetStringAsync($"{_jinaClient.BaseAddress}{searchItem.Link}");
            //var scrapeResponse = await _jinaClient.GetStreamAsync($"{_jinaClient.BaseAddress}{searchItem.Link}");

            //using var reader = new StreamReader(scrapeResponse);

            //char[] buffer = new char[1024];
            //int bytesRead = await reader.ReadBlockAsync(buffer, 0, MaxCharacterCount);
            //var content = new string(buffer, 0, bytesRead);


            var content = await GetFirstRenderedJinaContentAsync($"{_jinaClient.BaseAddress}{searchItem.Link}");
            //var content = await _jinaClient.GetStringAsync($"{_jinaClient.BaseAddress}{searchItem.Link}");

            //var chunks = TextChunker.SplitPlainTextParagraphs([content], 4000, 1000, request.Title, text => _tokenizer.CountTokens(text));

            var prompt = string.Format(Prompt, request.Title, content);

            var response = await Try.Execute(() =>
                _chatClient.GetResponseAsync<BookInformation>(prompt, s_jsonSerializerOptions, _chatOptions, true, cancellationToken),
                (ex) => _logger.LogInformation(ex, "Chat client error while attempting to resolve {Type}", nameof(BookInformation)));

            if (response.Error || response.Value.Result.Error)
            {
                yield return new() { Id = request.Id, Title = request.Title, Status = Status.BadResult, SourceLink = searchItem.Link };
                continue;
            }

            yield return new()
            {
                Id = request.Id,
                Status = Status.Complete,
                Data = new()
                {
                    Synopsis = response.Value.Result.Synopsis,
                    Genres = response.Value.Result.Genres,
                    MainAuthorName = response.Value.Result.MainAuthorName,
                    AuthorInformation = response.Value.Result.AuthorInformation,
                    MentionedAuthorNames = response.Value.Result.MentionedAuthorNames,
                },
                Title = request.Title,
                SourceLink = searchItem.Link
            };
        }
    }

    private async Task<GoogleSearch.Item?> GetFirstGoogleSearchResultAsync(Request request)
    {
        var url = $"{_googleClient.BaseAddress}&q={Uri.EscapeDataString(request.Title)}";
        var searchResponse = await _googleClient.GetFromJsonAsync<GoogleSearch.Result>(url);

        return searchResponse?.Items?.FirstOrDefault();
    }

    private async Task<string> GetFirstRenderedJinaContentAsync(string url)
    {
        var stream = await _jinaClient.GetStreamAsync(url);
        using var reader = new StreamReader(stream);

        var buffer = new char[4096];
        var result = new StringBuilder();

        while (result.Length < MaxCharacterCount)
        {
            var memory = buffer.AsMemory(0, Math.Min(buffer.Length, MaxCharacterCount - result.Length));
            var charsRead = await reader.ReadAsync(memory);
            if (charsRead == 0) break;

            result.Append(buffer.AsSpan(0, charsRead));
        }

        return result.ToString();
    }
}
