namespace Havensread.Connector;

public interface IWorkerCoordinator
{
    Task<bool> StartWorkerAsync(string workerName, CancellationToken workerToken = default);
    bool StopWorker(string workerName);
    bool StartWorkers();
    Task StopCoordinatorAsync();
    // Get Worker Status, lifetime, proccessed items, etc.
    // Multiple workers per type?
}
