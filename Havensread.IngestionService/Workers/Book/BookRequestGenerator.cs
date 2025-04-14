using Havensread.Data.App;
using Havensread.Data.Ingestion;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Havensread.IngestionService.Workers.Book;

public sealed class BookRequestGenerator
{
    private readonly static Channel<IEnumerable<BookIngestionHandler.Request>> s_channel =
        Channel.CreateBounded<IEnumerable<BookIngestionHandler.Request>>(new BoundedChannelOptions(1)
        {
            FullMode = BoundedChannelFullMode.Wait,
        });
    private readonly static List<IEnumerable<BookIngestionHandler.Request>> s_requests = new(WorkerDefaults.BatchSize);
    private readonly IServiceProvider _serviceProvider;
    private int _iteration = 0;

    public BookRequestGenerator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async IAsyncEnumerable<BookIngestionHandler.Request> GetRequestsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var request in await s_channel.Reader.ReadAsync(cancellationToken))
        {
            yield return request;
        }
    }

    public async Task BeginWritingProcessAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (s_requests.Count == 0)
            {
                _iteration = 0;
                var requestsBatch = await GetBookRequestsAsync().ToArrayAsync();
                s_requests.AddRange(requestsBatch.Chunk(WorkerDefaults.ChunkSize));
            }

            var requestsChunk = s_requests.Skip(_iteration).First();
            await s_channel.Writer.WriteAsync(requestsChunk, cancellationToken);
            _iteration++;

            if (_iteration * WorkerDefaults.ChunkSize == WorkerDefaults.BatchSize)
            {
                s_requests.Clear();
            }
        }
    }

    private async IAsyncEnumerable<BookIngestionHandler.Request> GetBookRequestsAsync()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        await using var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var ingestionDbContext = scope.ServiceProvider.GetRequiredService<IngestionDbContext>();

        var ingestedBookIds = await ingestionDbContext.Documents
            .AsNoTracking()
            .Where(x => x.Source == nameof(BookIngestionHandler) && x.Version == 0 && x.Timestamp != DateTimeOffset.MaxValue)
            .Select(x => x.Id)
            .ToArrayAsync();

        var books = appDbContext.Books
            .AsNoTracking()
            .Where(x => !ingestedBookIds.Contains(x.Id))
            .Take(WorkerDefaults.BatchSize)
            .Select(x => new BookIngestionHandler.Request(x.Id, x.Title, x.ISBN))
            .AsAsyncEnumerable();

        await foreach (var book in books)
        {
            yield return book;
        }
    }
}
