//using Havensread.IngestionService.Workers._Contracts;
//using MassTransit;
//using Microsoft.AspNetCore.SignalR;

//namespace Havensread.IngestionService;

//public sealed class JobProgressHub : Hub
//{
//    public async Task SendProgress(string jobId, int progress)
//    {
//        await Clients.All.SendAsync("ReceiveProgress", jobId, progress);
//    }

//    public async Task SendStatus(string jobId, string status)
//    {
//        await Clients.All.SendAsync("ReceiveStatus", jobId, status);
//    }



//}

//public sealed class WorkerHub : Hub
//{
//    private readonly IWorkerCoordinator _workerCoordinator;

//    public WorkerHub(IWorkerCoordinator workerCoordinator)
//    {
//        _workerCoordinator = workerCoordinator;
//    }

//    public async Task StartAsync()
//    {
//        _workerCoordinator.StartWorkers();

//        await Clients.All.SendAsync("ReceiveStatus", "Started workers");
//    }
//}

//public sealed class ProcessingCommandConsumer : IConsumer
//{

//}
