//using System.Text.Json;

//namespace Havensread.Api;

//public sealed class BookScraperService : BackgroundService
//{
//    private readonly ILogger<BookScraperService> _logger;
//    private readonly HttpClient _httpClient;
//    private readonly string _outputPath;

//    // Store books data
//    private List<Book> _books = [];

//    public BookScraperService(ILogger<BookScraperService> logger)
//    {
//        _logger = logger;
//        _httpClient = new HttpClient();
//        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 BookRecommendationSystem");
//        _outputPath = "books_data.json";
//    }

//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        _logger.LogInformation("Book Scraper Service is starting at: {time}", DateTimeOffset.Now);

//        try
//        {
//            // Example: Scrape multiple pages from a book catalog
//            for (int page = 1; page <= 5; page++)
//            {
//                if (stoppingToken.IsCancellationRequested) break;

//                await ScrapeBookCatalogPage(page, stoppingToken);

//                // Respect the site by waiting between requests
//                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
//            }

//            // Save all collected data
//            await SaveBooksDataAsync();

//            _logger.LogInformation("Book scraping completed successfully. Collected {count} books", _books.Count);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "An error occurred while scraping books");
//        }
//    }

//    private async Task ScrapeBookCatalogPage(int pageNumber, CancellationToken stoppingToken)
//    {
//        try
//        {
//            // Example URL - replace with actual book catalog URL
//            string url = $"https://example-book-site.com/catalog?page={pageNumber}";

//            _logger.LogInformation("Scraping page {page}", pageNumber);

//            string html = await _httpClient.GetStringAsync(url, stoppingToken);

//            var htmlDocument = new HtmlDocument();
//            htmlDocument.LoadHtml(html);

//            // Example: Find all book items on the page
//            var bookNodes = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'book-item')]");

//            if (bookNodes != null)
//            {
//                foreach (var bookNode in bookNodes)
//                {
//                    var book = new Book
//                    {
//                        Id = Guid.NewGuid().ToString(),
//                        Title = ExtractText(bookNode, ".//h3[@class='title']"),
//                        Author = ExtractText(bookNode, ".//span[@class='author']"),
//                        ISBN = ExtractText(bookNode, ".//span[@class='isbn']"),
//                        Description = ExtractText(bookNode, ".//div[@class='description']"),
//                        Categories = ExtractCategories(bookNode, ".//span[@class='category']"),
//                        Rating = ParseRating(ExtractText(bookNode, ".//div[@class='rating']")),
//                        CoverImageUrl = ExtractAttribute(bookNode, ".//img[@class='cover']", "src"),
//                        PageCount = ParseInt(ExtractText(bookNode, ".//span[@class='pages']")),
//                        PublicationDate = ParseDate(ExtractText(bookNode, ".//span[@class='publication-date']")),
//                        Publisher = ExtractText(bookNode, ".//span[@class='publisher']")
//                    };

//                    // Add to our collection
//                    _books.Add(book);

//                    // Log progress
//                    _logger.LogDebug("Scraped book: {title} by {author}", book.Title, book.Author);
//                }

//                _logger.LogInformation("Found {count} books on page {page}",
//                    bookNodes.Count, pageNumber);
//            }
//            else
//            {
//                _logger.LogWarning("No books found on page {page}", pageNumber);
//            }
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error scraping page {page}", pageNumber);
//        }
//    }

//    private string ExtractText(HtmlNode parentNode, string xpath)
//    {
//        var node = parentNode.SelectSingleNode(xpath);
//        return node != null ? node.InnerText.Trim() : string.Empty;
//    }

//    private string ExtractAttribute(HtmlNode parentNode, string xpath, string attributeName)
//    {
//        var node = parentNode.SelectSingleNode(xpath);
//        return node != null ? node.GetAttributeValue(attributeName, string.Empty) : string.Empty;
//    }

//    private List<string> ExtractCategories(HtmlNode parentNode, string xpath)
//    {
//        var categoryNodes = parentNode.SelectNodes(xpath);
//        var categories = new List<string>();

//        if (categoryNodes != null)
//        {
//            foreach (var categoryNode in categoryNodes)
//            {
//                categories.Add(categoryNode.InnerText.Trim());
//            }
//        }

//        return categories;
//    }

//    private double ParseRating(string ratingText)
//    {
//        if (double.TryParse(ratingText, out double rating))
//            return rating;
//        return 0;
//    }

//    private int ParseInt(string text)
//    {
//        if (int.TryParse(text, out int result))
//            return result;
//        return 0;
//    }

//    private DateTime? ParseDate(string dateText)
//    {
//        if (DateTime.TryParse(dateText, out DateTime date))
//            return date;
//        return null;
//    }

//    private async Task SaveBooksDataAsync()
//    {
//        string json = JsonSerializer.Serialize(_books, new JsonSerializerOptions
//        {
//            WriteIndented = true
//        });

//        await File.WriteAllTextAsync(_outputPath, json);
//        _logger.LogInformation("Books data saved to {path}", _outputPath);
//    }

//    public override async Task StopAsync(CancellationToken stoppingToken)
//    {
//        _logger.LogInformation("Book Scraper Service is stopping");

//        // Save any remaining data before stopping
//        if (_books.Count > 0)
//        {
//            await SaveBooksDataAsync();
//        }

//        await base.StopAsync(stoppingToken);
//    }
//}

//public class Book
//{
//    public string Id { get; set; }
//    public string Title { get; set; }
//    public string Author { get; set; }
//    public string ISBN { get; set; }
//    public string Description { get; set; }
//    public List<string> Categories { get; set; } = new List<string>();
//    public double Rating { get; set; }
//    public string CoverImageUrl { get; set; }
//    public int PageCount { get; set; }
//    public DateTime? PublicationDate { get; set; }
//    public string Publisher { get; set; }
//}
//}
