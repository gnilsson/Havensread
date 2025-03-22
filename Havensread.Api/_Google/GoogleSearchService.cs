//using Microsoft.Extensions.Options;
//using System.Text.Json;
//using System.Text.Json.Serialization;

//namespace Havensread.Api;

//public sealed class GoogleSearchService
//{
//    private readonly HttpClient _httpClient;


//    public GoogleSearchService(IHttpClientFactory httpClientFactory)
//    {
//        _httpClient = httpClientFactory.CreateClient("book-searcher");
//    }

//    public async Task<GoogleSearch.Result> SearchAsync(string query, int startIndex = 1, int numResults = 10)
//    {
//        ArgumentNullException.ThrowIfNull(query, nameof(query));

//        var url = $"&q={Uri.EscapeDataString(query)}&start={startIndex}&num={numResults}";
//        var response = await _httpClient.GetAsync(url);
//        response.EnsureSuccessStatusCode();

//        var content = await response.Content.ReadAsStreamAsync();
//        var searchResult = await JsonSerializer.DeserializeAsync<GoogleSearch.Result>(content, _serializer);
//        return searchResult!;
//    }
//}
