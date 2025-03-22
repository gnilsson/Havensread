//using static Havensread.Api.Ingestion.BookIngestionDataCollector;
//using System.Text.Json;

//namespace Havensread.Api.Ingestion;

//public class Test
//{

//    private async Task<string[]?> ExtractGenresAsync(
//        string title,
//        string prompt,
//        IEnumerable<ChunkContentDescription> descriptions,
//        CancellationToken cancellationToken)
//    {
//        var genresContent = descriptions.ToArray();

//        if (genresContent.Length == 0) return null;

//        List<string> genres = new();

//        if (genresContent.Length > 1)
//        {
//            // what are the odds.. I guess its possible
//            // ?
//        }

//        var genreContent = genresContent.First();

//        var p = string.Format(prompt, title, "Genres", genresContent[genreContent.ChunkIteration]);
//        var response = await _chatClient.GetResponseAsync<List<string>>(p, s_jsonSerializerOptions, null, true, cancellationToken);
//        genres.AddRange(response.Result);
//        return genres.ToArray();
//    }

//    private async Task<string?> ExtractSynopsisAsync(
//        string title,
//        string prompt,
//        IEnumerable<ChunkContentDescription> descriptions,
//        CancellationToken cancellationToken)
//    {
//        var synopsisContentArray = descriptions.ToArray();

//        if (synopsisContentArray.Length == 0) return null;

//        if (synopsisContentArray.Length == 1)
//        {
//            var p = string.Format(prompt, title, "Synopsis", synopsisContentArray[0].ChunkIteration);
//            var response = await _chatClient.GetResponseAsync(p, null, cancellationToken);
//            return response.Text;
//        }

//        List<string> synopses = new();

//        for (int i = 0; i < synopsisContentArray.Length; i++)
//        {
//            var p = string.Format(prompt, title, "Synopsis", synopsisContentArray[i].ChunkIteration);
//            var response = await _chatClient.GetResponseAsync(p, null, cancellationToken);
//            synopses.Add(response.Text);
//        }

//        var prompt2 = """
//            Your mission is to stitch together {0} chunks of text so that they form a coherent text.
//            The text you've been provided is a book synopsis of title {1}.
//            The text may contain overlapping segments, please remove the duplicated text.
//            Please try to keep the original text as it was written before the chunking process as best as possible.
//            The result must be a single piece of text that are identical with how the text was before it was chunked.

//            The text:
//            {2}
//            """;

//        // its never gonna be the case that synposis is > 8k token length
//        var chunksText = string.Join("\n\n", synopses);
//        var p2 = string.Format(prompt2, title, chunksText);
//        var response2 = await _chatClient.GetResponseAsync(p2, null, cancellationToken);
//        return response2.Text;
//    }

//    //private async Task<SearchResult> SearchAsync(
//    //    [Description("The search phrase to use for google")] string query,
//    //    [Description("Starting index"), DefaultValue(1)] int startIndex = 1,
//    //    [Description("Number of expected results"), DefaultValue(10)] int numResults = 10)
//    //{
//    //    var url = $"&q={Uri.EscapeDataString(query)}"; //&start={startIndex}&num={numResults}";
//    //    var response = await _googleClient.GetFromJsonAsync<GoogleSearch.Result>($"{_googleClient.BaseAddress}{url}");
//    //    if (response is null)
//    //    {
//    //        _logger.LogWarning("Google search failed for {Query}", query);
//    //        return null!;
//    //    }

//    //    // take only first
//    //    return new()
//    //    {
//    //        Kind = response.Kind,
//    //        TotalItems = response.SearchInformation.TotalResults,
//    //        Items = response.Items?.Select(x => new SearchItemResult
//    //        {
//    //            Kind = x.Kind,
//    //            Title = x.Title,
//    //            Link = x.Link
//    //        }).ToArray() ?? Array.Empty<SearchItemResult>()
//    //    };
//    //}

//    //private async Task<string> NavigateAndReadAsync(
//    //    [Description("The url address of a site to read")] string url,
//    //    [Description("The ID of the book")] Guid id)
//    //{
//    //    var response = await _jinaClient.GetStringAsync($"{_jinaClient.BaseAddress}{url}");
//    //    if (string.IsNullOrWhiteSpace(response))
//    //    {
//    //        _logger.LogWarning("Jina failed to read content from {Url}", url);
//    //        return string.Empty;
//    //    }
//    //    // Chunking?

//    //    // need to do some processing here

//    //    return response;
//    //}



//    public sealed record SearchResult
//    {
//        public required string Kind { get; init; }

//        public string? TotalItems { get; init; }

//        public required ICollection<SearchItemResult> Items { get; init; }
//    }

//    public sealed record SearchItemResult
//    {
//        public required string Kind { get; set; }

//        public required string Title { get; set; }

//        public required string Link { get; set; }
//    }

//    public sealed record ChunkContentDescription
//    {
//        public required int ChunkIteration { get; init; }
//        public required string Summary { get; init; }
//        public bool ContentHasSynopsis { get; init; }
//        public bool ContentHasGenres { get; init; }
//        public bool ContentHasReviews { get; init; }
//    }
//}



////List<ChunkContentDescription> descriptions = new();

////for (int i = 0; i < chunks.Count; i++)
////{
////    var chunk = chunks[i];
////    var p = string.Format(prompt, chunks.Count, i, chunk);
////    var response = await _chatClient.GetResponseAsync<ChunkContentDescription>(p, s_jsonSerializerOptions, _chatOptions, true, cancellationToken);
////    descriptions.Add(response.Result);
////}

////var prompt2 = """
////    Your mission is to extract text that describes the book {0}.
////    What we are looking to find is the {1} of the book and the current piece of text has been identified to contain this content.
////    Please extract the relevant text verbatim, and ignore all the text that does not fall under the category of {1}.

////    Text:
////    {2}
////    """;


//// note:
//// currently not using the summary of the chunk
//// this could however be useful when you want to be aware of context between chunks


////var synopsis = await ExtractSynopsisAsync(request.Title, prompt2, descriptions.Where(x => x.ContentHasSynopsis), cancellationToken);

////var genres = await ExtractGenresAsync(request.Title, prompt2, descriptions.Where(x => x.ContentHasGenres), cancellationToken);

////if (synopsis is null)
////{
////    yield return new Response { Status = Status.BadResult, SourceLink = searchItem.Link };
////    continue;
////}

////yield return new Response
////{
////    Status = Status.Complete,
////    Data = new Data
////    {
////        Synopsis = synopsis,
////        Genres = genres ?? []
////    },
////    SourceLink = searchItem.Link,
////};

////foreach (var description in descriptions)
////{
////    // if only one ...
////    if (description.ContentHasSynopsis)
////    {



////    }
////    if (description.ContentHasGenres)
////    {
////        var p = string.Format(prompt2, request.Title, "Genres", chunks[description.ChunkIteration]);
////        var response = await _chatClient.GetResponseAsync<List<string>>(p, _jsonSerializerOptions, null, true, cancellationToken);
////        genres.AddRange(response.Result.Where(x => !genres.Contains(x)));
////    }
////    if (description.ContentHasReviews)
////    {
////        // reviews
////    }
////}

//////var chunks = Array.Empty<string>().ToList();

////var prompt = """
////    You are a part of a process that is reading and understanding a large piece of text that comes from a page on the internet about a specific book divided into {0} chunks, you are currently on process iteration number {1}.
////    Your goal is to categorize contents in the next in to a few predetermined categories. We know before hand that the text most probably have these categories but they might not be explicitly stated as such or divided in to these categories.
////    Some chunks will have the same occurring category overlapping between chunks,  some  might have no category, and some might have multiple.

////    The categories we are looking for is: Synopsis, Genres, Reviews.

////    Please also provide a brief description or summary about the contents of the text chunk.

////    Text chunk:
////    {2}
////    """;




////private const string Prompt = """
////        Book Information Retrieval Agent
////        You are a specialized agent designed to find and extract detailed information about books from the web.
////        Your primary goal is to provide comprehensive, accurate information about books based on titles and/or ISBN numbers.

////        Use your provided tools to search the web for information about the given book, and then use the url from the search result to navigate to the page containing the information.
////        The information you are interested in is the synopsis, summary, or description, and the genres of the book.

////        Use one of the following sites as basis of your search:
////        Name: Goodreads
////        URL: https://www.goodreads.com/
////        Additional instruction: To search on this site it is prefered to use the book title

////        The book we are interested in:
////        ID: {0}
////        Title: {1}
////        ISBN: {2}
////        """;


////Tools = [AIFunctionFactory.Create(SearchAsync), AIFunctionFactory.Create(NavigateAndReadAsync)],



//var chatClient = scope.ServiceProvider.GetRequiredService<IChatClient>();
//var chatOptions = new ChatOptions
//{
//    Tools = [AIFunctionFactory.Create(SearchAsync)]
//};
//var clientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
//var httpClient = clientFactory.CreateClient("book-searcher");


// Go through the books one by one
// Begin ingestion process

//foreach (var book in books)
//{
//    //var information = await RetrieveBookInformationAsync(book);
//}

// 1. Ask agent to retrieve data about using title, isbn, ...
// the data we need is synopsis, genres, ...

// 2. Generate embeddings for the synopsis, genres, and metadata description
// book information from one book corresponds to one ingestion document
// different types of informations are records,
// and texts over N amount of characters are truncated into records
// if we are running locally it could be wise to store the embeddings in a file

// 3. Load the embeddings into vector store

// 4. Add document with records to the ingestion db