using Havensread.Data.Ingestion;
using Havensread.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Havensread.IngestionService.Workers.Book;

internal sealed class BookIngestionWorker : IWorker
{
    private readonly static Meter s_meter = new(nameof(BookIngestionWorker));
    private readonly static Counter<int> s_requestsCompleted = s_meter.CreateCounter<int>("requests.completed");
    private readonly static Histogram<double> s_processLatency = s_meter.CreateHistogram<double>("process.latency.ms");
    private readonly static Counter<int> s_totalProcesses = s_meter.CreateCounter<int>("total.processes");

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

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _ = _requestGenerator.BeginWritingProcessAsync(cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            var stopwatch = Stopwatch.StartNew();

            var requests = _requestGenerator.GetRequestsAsync(cancellationToken);

            await using var scope = _serviceProvider.CreateAsyncScope();

            var handler = scope.ServiceProvider.GetRequiredService<BookIngestionHandler>();
            var responses = handler.ExecuteAsync(requests, cancellationToken);

            await using var ingestionContext = scope.ServiceProvider.GetRequiredService<IngestionDbContext>();
            var points = await CreatePointsAndIngestionDocumentsAsync(ingestionContext, responses)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

            if (points.Length == 0) break;

            await Task.WhenAll(
                _qdrantClient.UpsertAsync(SourceName.Books, points),
                ingestionContext.SaveChangesAsync()).ConfigureAwait(false);

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") is "Development")
            {
                await LocalStorageHelper
                    .WriteJsonAsync(points, p => p.Id.Uuid, DirectoryName.Points, CancellationToken.None)
                    .ConfigureAwait(false);
            }

            s_requestsCompleted.Add(points.Length);
            s_processLatency.Record(stopwatch.ElapsedMilliseconds);
            s_totalProcesses.Add(1);
        }
    }

    private async IAsyncEnumerable<PointStruct> CreatePointsAndIngestionDocumentsAsync(
        IngestionDbContext ingestionContext,
        IAsyncEnumerable<BookIngestionHandler.Response> responses)
    {
        // note:
        // should batch the db query
        await foreach (var response in responses)
        {
            if (!response.Success || response.Data.Error)
            {
                _logger.LogInformation("Failed to ingest book {Title}.", response.Title);
                continue;
            }

            var synopsisVector = await _embeddingGenerator
                .GenerateEmbeddingVectorAsync(response.Data.Synopsis)
                .ConfigureAwait(false);

            var pointId = Guid.NewGuid();
            var point = new PointStruct
            {
                Id = new PointId { Uuid = pointId.ToString() },
                Vectors = synopsisVector.ToArray(),
                Payload =
                {
                    ["title"] = response.Title,
                    ["synopsis"] = response.Data.Synopsis,
                    ["genres"] = response.Data.Genres.ToArray(),
                    ["author"] = response.Data.MainAuthorName,
                    ["mentioned_authors"] = response.Data.MentionedAuthorNames?.ToArray() ?? Array.Empty<string>(),
                    ["source_link"] = response.SourceLink
                }
            };

            yield return point;

            var record = new IngestedRecord
            {
                Id = pointId,
                DocumentId = response.Id,
                DocumentSource = nameof(BookIngestionHandler),
                SourceLink = response.SourceLink,
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
