using Havensread.Connector;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Havensread.Connector;

public sealed class WorkerHub : Hub<IWorkerHub>
{
    private readonly record struct WorkerOperation(string WorkerName, string Operation, bool Result);

    public sealed class ServerMethodName
    {
        public const string StartWorkers = nameof(WorkerHub.StartWorkersAsync);
        public const string StartWorker = nameof(WorkerHub.StartWorkerAsync);
        public const string StopWorker = nameof(WorkerHub.StopWorker);
        public const string StopCoordinator = nameof(WorkerHub.StopCoordinatorAsync);
        public const string GetWorkerStates = nameof(WorkerHub.GetWorkerStates);
    }

    private readonly IWorkerCoordinator _workerCoordinator;

    public WorkerHub(IWorkerCoordinator workerCoordinator)
    {
        _workerCoordinator = workerCoordinator;
    }

    public async Task StartWorkersAsync()
    {
        var result = _workerCoordinator.StartWorkers();
        await Clients.Caller.SendResultAsync(result);
    }

    public async Task StartWorkerAsync(string workerName)
    {
        var result = _workerCoordinator.StartWorker(workerName);
        await Clients.Caller.SendResultAsync(result);
    }

    public async Task StopWorker(string workerName)
    {
        var result = _workerCoordinator.StopWorker(workerName);
        await Clients.Caller.SendResultAsync(result);
    }

    public async Task StopCoordinatorAsync()
    {
        Context.Abort();
        await _workerCoordinator.StopCoordinatorAsync();
    }

    public async Task GetWorkerStates()
    {
        var workerStates = _workerCoordinator.GetWorkerStates();
        //await Clients.Caller.SendResultAsync(workerStates);
    }
}
