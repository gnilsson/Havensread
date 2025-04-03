using Havensread.Data.App;
using Havensread.Data.Ingestion;
using Havensread.DataIngestor;
using Havensread.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Globalization;
using System.Text.Json;

namespace Havensread.MigrationService;

public static class DatabaseSeedHelper
{
    private static readonly string[] s_publicationDateFormats = ["M/d/yyyy", "MM/dd/yyyy"];
    private static readonly JsonSerializerOptions s_jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static async Task GenerateIngestionSeedDataAsync(
        IServiceProvider serviceProvider,
        IngestionDbContext context,
        string solutionDir,
        CancellationToken cancellationToken)
    {
        await foreach (var document in LocalStorageHelper.ReadFromJsonDiskAsync<IngestedDocument>(
            DirectoryName.IngestedDocuments,
            s_jsonOptions,
            cancellationToken))
        {
            context.Documents.Add(document);
        }

        var points = await LocalStorageHelper
            .ReadFromJsonDiskAsync<PointStruct>(DirectoryName.Points, s_jsonOptions, cancellationToken)
            .ToArrayAsync();

        if (points.Length == 0) return;

        var qdrantClient = serviceProvider.GetRequiredService<QdrantClient>();
        if (!await qdrantClient.CollectionExistsAsync(SourceName.Books))
        {
            await qdrantClient.CreateCollectionAsync(
                SourceName.Books,
                vectorsConfig: new VectorParams { Size = 1536, Distance = Distance.Cosine });
        }
        await qdrantClient.UpsertAsync(SourceName.Books, points);
    }

    public static async Task GenerateAppSeedDataAsync(AppDbContext context, string solutionDir, CancellationToken cancellationToken)
    {
        var jsonDir = Path.Combine(solutionDir, DirectoryName.SeedData, DirectoryName.Kaggle, DirectoryName.Books);
        if (!Directory.Exists(jsonDir)) return;

        List<string> existingAuthors = [];
        await foreach (var kaggleBook in LocalStorageHelper.ReadFromJsonDiskAsync<Kaggle.Book>(jsonDir, s_jsonOptions, cancellationToken))
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

    private static DateTime? ParsePublicationDate(string? publicationDate)
    {
        if (DateTime.TryParseExact(
            publicationDate,
            s_publicationDateFormats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var date))
        {
            return DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }

        return null;
    }
}
