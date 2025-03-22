using Havensread.Data.App;
using Havensread.Data.Ingestion;
using Havensread.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Havensread.Api.Ingestion;

public sealed class IngestionBackgroundService : MonitoredBackgroundService
{
    private readonly QdrantClient _qdrantClient;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IngestionBackgroundService> _logger;

    public IngestionBackgroundService(
        QdrantClient qdrantClient,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        IServiceProvider serviceProvider,
        ILogger<IngestionBackgroundService> logger) : base(logger)
    {
        _qdrantClient = qdrantClient;
        _embeddingGenerator = embeddingGenerator;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override string ActivitySourceName { get; } = "Book data ingestion";
    protected override string ServiceName { get; } = nameof(IngestionBackgroundService);

    private const int ChunkSize = 10;
    private const int BatchSize = ChunkSize * 10;

    protected override async Task RunAsync(CancellationToken stoppingToken)
    {
        if (!await _qdrantClient.CollectionExistsAsync(SourceName.Books))
        {
            await _qdrantClient.CreateCollectionAsync(
                SourceName.Books,
                vectorsConfig: new VectorParams { Size = 1536, Distance = Distance.Cosine });
        }

        var requestBatch = new BookIngestionDataCollector.Request[BatchSize];
        var count = BatchSize;

        while (!stoppingToken.IsCancellationRequested)
        {
            if (count == BatchSize)
            {
                requestBatch = await GetBookIngestionRequestsAsync().ToArrayAsync(stoppingToken);

                if (requestBatch.Length == 0)
                {
                    _logger.LogInformation("No books to ingest.");
                    break;
                }
                count = 0;
            }

            var requests = requestBatch.Skip(count).Take(ChunkSize);
            count += ChunkSize;

            await using var scope = _serviceProvider.CreateAsyncScope();
            var handler = scope.ServiceProvider.GetRequiredService<BookIngestionDataCollector>();

            try
            {
                var response = handler.ExecuteAsync(requests, stoppingToken);

                var points = await CreatePointsAndUpdateIngestionDbAsync(scope, response).ToArrayAsync();

                await _qdrantClient.UpsertAsync(SourceName.Books, points);

            }
            catch (Exception e)
            {
                // probably reached request limit
                _logger.LogError(e, "An error occurred while ingesting books.");
                throw;
            }

            requestBatch = requestBatch.Except(requests).ToArray();
        }
    }

    private async IAsyncEnumerable<PointStruct> CreatePointsAndUpdateIngestionDbAsync(
        AsyncServiceScope scope,
        IAsyncEnumerable<BookIngestionDataCollector.Response> response)
    {
        await using var ingestionContext = scope.ServiceProvider.GetRequiredService<IngestionDbContext>();

        await foreach (var result in response)
        {
            if (!result.Success || result.Data.Error)
            {
                _logger.LogWarning("Failed to ingest book {Title}.", result.Title);
                continue;                        // do something
            }

            var synopsisVector = await _embeddingGenerator.GenerateEmbeddingVectorAsync(result.Data.Synopsis);

            var point = new PointStruct
            {
                Id = new PointId { Uuid = result.SourceLink },
                Vectors = synopsisVector.ToArray(),
                Payload =
                {
                    ["title"] = result.Title,
                    ["synopsis"] = result.Data.Synopsis,
                    ["genres"] = result.Data.Genres.ToArray(),
                    ["author"] = result.Data.MainAuthorName,
                    ["mentioned_authors"] = result.Data.MentionedAuthorNames?.ToArray() ?? Array.Empty<string>(),
                }
            };

            yield return point;

            var record = new IngestedRecord
            {
                Id = result.SourceLink,
                DocumentId = result.Id,
                DocumentSource = nameof(BookIngestionDataCollector),
            };

            var existingDocument = await ingestionContext.Documents.FindAsync([result.Id, nameof(BookIngestionDataCollector)]);

            if (existingDocument is null)
            {
                ingestionContext.Documents.Add(new IngestedDocument
                {
                    Id = result.Id,
                    Source = nameof(BookIngestionDataCollector),
                    Timestamp = DateTimeOffset.UtcNow,
                    Records = [record]
                });
            }
            else
            {
                existingDocument.Records.Add(record);
                existingDocument.Timestamp = DateTimeOffset.UtcNow;
            }
        }

        await ingestionContext.SaveChangesAsync();
    }

    private async IAsyncEnumerable<BookIngestionDataCollector.Request> GetBookIngestionRequestsAsync()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        using var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        using var ingestionDbContext = scope.ServiceProvider.GetRequiredService<IngestionDbContext>();

        var ingestedBookIds = await ingestionDbContext.Documents
            .AsNoTracking()
            .Where(x => x.Source == nameof(BookIngestionDataCollector))
            .Select(x => x.Id)
            .ToArrayAsync();

        var books = appDbContext.Books
            .AsNoTracking()
            .Where(x => !ingestedBookIds.Contains(x.Id))
            .Take(BatchSize)
            .Select(x => new BookIngestionDataCollector.Request(x.Id, x.Title, x.ISBN))
            .AsAsyncEnumerable();

        await foreach (var book in books)
        {
            yield return book;
        }
    }
}
