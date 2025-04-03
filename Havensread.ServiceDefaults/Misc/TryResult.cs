using System.Diagnostics.CodeAnalysis;

namespace Havensread.IngestionService.Workers.Book;

public sealed class TryResult<T>
{
    [MemberNotNullWhen(false, nameof(Value))]
    public bool Error { get; init; }
    public T? Value { get; init; }
}

//public sealed partial class BookIngestionHandler
//{
    //public async IAsyncEnumerable<Response> ExecuteAsync(
    //    IAsyncEnumerable<Request> requests,
    //    [EnumeratorCancellation] CancellationToken cancellationToken)
    //{
    //    List<Response> responses = [];

    //    var options = new ParallelOptions
    //    {
    //        //Environment.ProcessorCount, with multiple workers this might become sketchy to parallelize
    //        // wonder if if i can derive a good number from processor count and active worker count
    //        MaxDegreeOfParallelism = 1,
    //        CancellationToken = cancellationToken
    //    };

    //    await Parallel.ForEachAsync(requests, options, async (request, token) =>
    //    {
    //        var url = $"{_googleClient.BaseAddress}&q={Uri.EscapeDataString(request.Title)}";
    //        var searchResponse = await _googleClient.GetFromJsonAsync<GoogleSearch.Result>(url);
    //        var searchItem = searchResponse?.Items?.FirstOrDefault();

    //        if (searchItem is null)
    //        {
    //            responses.Add(new() { Id = request.Id, Title = request.Title, Status = Status.BadQuery });
    //            return;
    //        }



    //        //var content = await _jinaClient.GetStringAsync($"{_jinaClient.BaseAddress}{searchItem.Link}");
    //        //var scrapeResponse = await _jinaClient.GetStreamAsync($"{_jinaClient.BaseAddress}{searchItem.Link}");

    //        //using var reader = new StreamReader(scrapeResponse);

    //        //char[] buffer = new char[1024];
    //        //int bytesRead = await reader.ReadBlockAsync(buffer, 0, MaxCharacterCount);
    //        //var content = new string(buffer, 0, bytesRead);


    //        //var content = await GetJinaContentAsync($"{_jinaClient.BaseAddress}{searchItem.Link}");
    //        var content = await _jinaClient.GetStringAsync($"{_jinaClient.BaseAddress}{searchItem.Link}");

    //        var chunks = TextChunker.SplitPlainTextParagraphs([content], 5000, 1000, request.Title, text => _tokenizer.CountTokens(text));

    //        var prompt = string.Format(Prompt, request.Title, chunks[0]);
    //        var response = await TryAsync(() =>
    //            _chatClient.GetResponseAsync<BookInformation>(prompt, s_jsonSerializerOptions, _chatOptions, true, cancellationToken));

    //        if (response.Error || response.Value.Result.Error)
    //        {
    //            responses.Add(new() { Id = request.Id, Title = request.Title, Status = Status.BadResult, SourceLink = searchItem.Link });
    //        }

    //        responses.Add(new()
    //        {
    //            Id = request.Id,
    //            Status = Status.Complete,
    //            Data = new()
    //            {
    //                Synopsis = response.Value.Result.Synopsis,
    //                Genres = response.Value.Result.Genres,
    //                AuthorInformation = response.Value.Result.AuthorInformation
    //            },
    //            Title = request.Title,
    //            SourceLink = searchItem.Link
    //        });
    //    });

    //    foreach (var response in responses)
    //    {
    //        yield return response;
    //    }
    //}


//}
