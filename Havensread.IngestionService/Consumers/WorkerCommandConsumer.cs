﻿//using Havensread.Connector;
//using Havensread.Connector.Messages;
//using Havensread.Data.App;
//using Havensread.Data.Ingestion;
//using Havensread.IngestionService.Workers;
//using MassTransit;
//using Microsoft.EntityFrameworkCore;

//namespace Havensread.IngestionService.Consumer;

//public sealed class WorkerCommandConsumer : IConsumer<WorkerMessage>
//{
//    private readonly AppDbContext _appDbContext;
//    private readonly IngestionDbContext _ingestionDbContext;
//    private readonly IWorkerCoordinatorSentry _sentry;
//    private readonly BoundedChannelQueue<BookIngestionHandler.Request> _queue;

//    public WorkerCommandConsumer(
//        AppDbContext appDbContext,
//        IngestionDbContext ingestionDbContext,
//        IWorkerCoordinatorSentry sentry,
//        BoundedChannelQueue<BookIngestionHandler.Request> queue)
//    {
//        _appDbContext = appDbContext;
//        _ingestionDbContext = ingestionDbContext;
//        _sentry = sentry;
//        _queue = queue;
//    }

//    public async Task Consume(ConsumeContext<WorkerMessage> context)
//    {
//        var state = _sentry.GetWorkerState(context.Message.WorkerName);

//        //if (state)

//        var requests = await GetBookIngestionRequestsAsync(context.Message.BatchSize).ToArrayAsync();

//        await _queue.EnqueueAsync(requests);



//    }

//    private async IAsyncEnumerable<BookIngestionHandler.Request> GetBookIngestionRequestsAsync(int? take = null)
//    {
//        var ingestedBookIds = await _ingestionDbContext.Documents
//            .AsNoTracking()
//            .Where(x => x.Source == nameof(BookIngestionHandler) && x.Version == 0)
//            .Select(x => x.Id)
//            .ToArrayAsync();

//        var books = _appDbContext.Books
//            .AsNoTracking()
//            .Where(x => !ingestedBookIds.Contains(x.Id))
//            .Take(take ?? WorkerDefaults.BatchSize)
//            .Select(x => new BookIngestionHandler.Request(x.Id, x.Title, x.ISBN))
//            .AsAsyncEnumerable();

//        await foreach (var book in books)
//        {
//            yield return book;
//        }
//    }
//}
