using Havensread.Data.App;
using Havensread.Data.Ingestion;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Havensread.IngestionService.Workers.Book;

public sealed class BookRequestGenerator
{
    private readonly static Channel<IEnumerable<BookIngestionHandler.Request>> s_channel =
        Channel.CreateBounded<IEnumerable<BookIngestionHandler.Request>>(new BoundedChannelOptions(WorkerDefaults.QueueSize)
        {
            FullMode = BoundedChannelFullMode.Wait,
        });
    private readonly static List<IEnumerable<BookIngestionHandler.Request>> s_requests = new(WorkerDefaults.BatchSize);
    private readonly IServiceProvider _serviceProvider;

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
        var iteration = WorkerDefaults.BatchSize / WorkerDefaults.ChunkSize;

        while (!cancellationToken.IsCancellationRequested)
        {
            if (iteration * WorkerDefaults.ChunkSize == WorkerDefaults.BatchSize)
            {
                iteration = 0;

                var requestsBatch = await GetBookRequestsAsync().ToArrayAsync();

                s_requests.Clear();
                s_requests.AddRange(requestsBatch.Chunk(WorkerDefaults.ChunkSize));
            }

            var requestsChunk = s_requests.Skip(iteration).First();

            await s_channel.Writer.WriteAsync(requestsChunk, cancellationToken);

            iteration++;
        }
    }

    private async IAsyncEnumerable<BookIngestionHandler.Request> GetBookRequestsAsync()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        await using var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var ingestionDbContext = scope.ServiceProvider.GetRequiredService<IngestionDbContext>();

        var ingestedBookIds = await ingestionDbContext.Documents
            .AsNoTracking()
            .Where(x => x.Source == nameof(BookIngestionHandler) && x.Version == 0)
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
