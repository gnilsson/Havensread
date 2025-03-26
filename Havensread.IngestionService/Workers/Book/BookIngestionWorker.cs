using Havensread.Data.Ingestion;
using Havensread.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Diagnostics;
using System.Text.Json;

namespace Havensread.IngestionService.Workers.Book;

internal sealed class BookIngestionWorker : IWorker
{
    private static readonly ActivitySource s_activitySource = new(nameof(BookIngestionWorker));
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new()
    {
        WriteIndented = true,
    };

    private readonly IServiceProvider _serviceProvider;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly QdrantClient _qdrantClient;
    private readonly BookRequestGenerator _requestGenerator;
    private readonly ILogger<BookIngestionWorker> _logger;

    public BookIngestionWorker(
        IServiceProvider serviceProvider,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        QdrantClient qdrantClient,
        BookRequestGenerator requestGenerator,
        ILogger<BookIngestionWorker> logger)
    {
        _qdrantClient = qdrantClient;
        _embeddingGenerator = embeddingGenerator;
        _serviceProvider = serviceProvider;
        _requestGenerator = requestGenerator;
        _logger = logger;
    }

    public string Name { get; } = nameof(BookIngestionWorker);

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _ = Task.Run(async () =>
        {
            await _requestGenerator.BeginWritingProcessAsync(stoppingToken);
        });

        while (!stoppingToken.IsCancellationRequested)
        {
            var requests = _requestGenerator.GetRequestsAsync(stoppingToken);

            await using var scope = _serviceProvider.CreateAsyncScope();
            var handler = scope.ServiceProvider.GetRequiredService<BookIngestionHandler>();

            var responses = handler.ExecuteAsync(requests, stoppingToken);

            await using var ingestionContext = scope.ServiceProvider.GetRequiredService<IngestionDbContext>();

            var points = await CreatePointsAndIngestionDocumentsAsync(ingestionContext, responses).ToArrayAsync(stoppingToken);

            if (points.Length == 0) break;

            await Task.WhenAll(
                _qdrantClient.UpsertAsync(SourceName.Books, points),
                ingestionContext.SaveChangesAsync());


            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") is "Development")
            {
                await LocalStorageHelper.WriteToJsonDiskAsync(points, p => p.Id.Uuid, "points", CancellationToken.None);
            }
        }
    }

    private async IAsyncEnumerable<PointStruct> CreatePointsAndIngestionDocumentsAsync(
        IngestionDbContext ingestionContext,
        IAsyncEnumerable<BookIngestionHandler.Response> responses)
    {
        await foreach (var response in responses)
        {
            if (!response.Success || response.Data.Error)
            {
                _logger.LogInformation("Failed to ingest book {Title}.", response.Title);
                continue;
            }

            var synopsisVector = await _embeddingGenerator.GenerateEmbeddingVectorAsync(response.Data.Synopsis);

            var point = new PointStruct
            {
                Id = new PointId { Uuid = response.SourceLink },
                Vectors = synopsisVector.ToArray(),
                Payload =
                {
                    ["title"] = response.Title,
                    ["synopsis"] = response.Data.Synopsis,
                    ["genres"] = response.Data.Genres.ToArray(),
                    ["author"] = response.Data.MainAuthorName,
                    ["mentioned_authors"] = response.Data.MentionedAuthorNames?.ToArray() ?? Array.Empty<string>(),
                }
            };

            yield return point;

            var record = new IngestedRecord
            {
                Id = response.SourceLink,
                DocumentId = response.Id,
                DocumentSource = nameof(BookIngestionHandler),
            };

            var existingDocument = await ingestionContext.Documents.FindAsync([response.Id, nameof(BookIngestionHandler)]);

            if (existingDocument is not null)
            {
                existingDocument.Records.Add(record);
                existingDocument.Timestamp = DateTimeOffset.UtcNow;
                existingDocument.Version++;
                continue;
            }

            ingestionContext.Documents.Add(new IngestedDocument
            {
                Id = response.Id,
                Source = nameof(BookIngestionHandler),
                Timestamp = DateTimeOffset.UtcNow,
                Records = [record]
            });
        }
    }
}
