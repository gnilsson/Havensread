using Microsoft.AspNetCore.SignalR;

namespace Havensread.Connector;

public sealed class WorkerHub : Hub<IWorkerHubClient>
{
    public sealed class ServerMethodName
    {
        public const string StartWorkers = nameof(WorkerHub.StartWorkersAsync);
        public const string StartWorker = nameof(WorkerHub.StartWorkerAsync);
        public const string StopWorker = nameof(WorkerHub.StopWorker);
        public const string GetWorkerDatas = nameof(WorkerHub.GetWorkerDatas);
    }

    private readonly IWorkerCoordinator _coordinator;

    public WorkerHub(IWorkerCoordinator workerCoordinator)
    {
        _coordinator = workerCoordinator;
    }

    public async Task StartWorkersAsync()
    {
        foreach (var worker in _coordinator.GetWorkerDatas())
        {
            if (worker.State is Worker.State.Running) continue;

            _coordinator.StartWorker(worker.Name);
        }

        await Clients.Caller.SendResultAsync(true);
    }

    public async Task StartWorkerAsync(string workerName)
    {
        var result = _coordinator.StartWorker(workerName);

        await Clients.Caller.SendResultAsync(result);
    }

    public async Task StopWorker(string workerName)
    {
        var result = _coordinator.StopWorker(workerName);

        await Clients.Caller.SendResultAsync(result);
    }

    public IEnumerable<Worker.Data> GetWorkerDatas() => _coordinator.GetWorkerDatas();

    //public async Task BroadcastWorkerDatas() =>
    //    await Clients.All.SendWorkerDatasAsync(_coordinator.GetWorkerDatas());
}
