using Havensread.Data;
using Havensread;
using Havensread.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Havensread.DataIngestor;

namespace Havensread.MigrationService;

public static class DatabaseSeedHelper
{
    private static readonly string[] _publicationDateFormats = ["M/d/yyyy", "MM/dd/yyyy"];

    public static async Task SeedDataAsync(DbContext context, string solutionDir, CancellationToken cancellationToken)
    {
        var booksDir = Path.Combine(solutionDir, "seeddata", "kaggle", "booksJson");

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, };
        List<string> existingAuthors = [];

        await foreach (var kaggleBook in ReadBooksFromJsonFilesAsync(booksDir, options, cancellationToken))
        {
            var authorNames = kaggleBook.Authors.Split('/').ToArray();
            existingAuthors.AddRange(authorNames);
            var authors = authorNames.Select(a => new Author { Name = a }).ToArray();

            var book = new Book
            {
                SourceId = kaggleBook.BookID,
                Title = kaggleBook.Title,
                Authors = authors,
                AverageRating = kaggleBook.AverageRating,
                RatingsCount = kaggleBook.RatingsCount,
                Publisher = kaggleBook.Publisher,
                ISBN = kaggleBook.ISBN,
                ISBN13 = kaggleBook.ISBN13,
                LanguageCode = kaggleBook.LanguageCode,
                NumPages = kaggleBook.NumPages,
                TextReviewsCount = kaggleBook.TextReviewsCount,
                PublicationDate = ParsePublicationDate(kaggleBook.PublicationDate)
            };

            context.Set<Author>().AddRange(authors.Where(a => !existingAuthors.Contains(a.Name)));
            context.Set<Book>().Add(book);
        }
    }

    private static async Task SeedBooksAsync(DbContext context, string booksDir, CancellationToken cancellationToken)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, };
        List<string> existingAuthors = [];

        await foreach (var kaggleBook in ReadBooksFromJsonFilesAsync(booksDir, options, cancellationToken))
        {
            var authorNames = kaggleBook.Authors.Split('/').ToArray();
            var authors = authorNames.Select(a => new Author { Name = a }).ToArray();

            var book = new Book
            {
                SourceId = kaggleBook.BookID,
                Title = kaggleBook.Title,
                Authors = authors,
                AverageRating = kaggleBook.AverageRating,
                RatingsCount = kaggleBook.RatingsCount,
                Publisher = kaggleBook.Publisher,
                ISBN = kaggleBook.ISBN,
                ISBN13 = kaggleBook.ISBN13,
                LanguageCode = kaggleBook.LanguageCode,
                NumPages = kaggleBook.NumPages,
                TextReviewsCount = kaggleBook.TextReviewsCount,
                PublicationDate = ParsePublicationDate(kaggleBook.PublicationDate)
            };

            context.Set<Author>().AddRange(authors.Where(a => !existingAuthors.Contains(a.Name)));
            context.Set<Book>().Add(book);
            existingAuthors.AddRange(authorNames);
        }
    }

    private static async IAsyncEnumerable<Kaggle.Book> ReadBooksFromJsonFilesAsync(
        string jsonDir,
        JsonSerializerOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var file in Directory.EnumerateFiles(jsonDir, "*.json"))
        {
            await using var stream = File.OpenRead(file);
            var book = await JsonSerializer.DeserializeAsync<Kaggle.Book>(stream, options, cancellationToken);
            if (book is not null) yield return book;
        }
    }

    private static DateTime? ParsePublicationDate(string? publicationDate)
    {
        if (DateTime.TryParseExact(
            publicationDate,
            _publicationDateFormats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var date))
        {
            return DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }

        return null;
    }
}
