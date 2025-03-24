using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Havensread.Connector;

public interface IWorkerHub
{
    Task StartWorkersAsync();
    Task StopCoordinatorAsync();
}

public sealed class WorkerHub : Hub<IWorkerHub>
{
    public sealed class MethodName
    {
        public const string StartWorkers = nameof(StartWorkersAsync);
        public const string Stop = nameof(StopCoordinatorAsync);


    }

    private readonly IWorkerCoordinator _workerCoordinator;

    public WorkerHub(IWorkerCoordinator workerCoordinator)
    {
        _workerCoordinator = workerCoordinator;
    }

    public async Task StartWorkersAsync()
    {
        _workerCoordinator.StartWorkers();

        //Clients.All.
        //await Clients.All..SendAsync("ReceiveStatus", "Started workers");
    }

    public async Task StopCoordinatorAsync()
    {
        Context.Abort();
        await _workerCoordinator.StopCoordinatorAsync();

        //await Clients.All.SendAsync("ReceiveStatus", "Stopped workers");
    }
}
