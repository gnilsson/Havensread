namespace Havensread.Connector;

public interface IWorkerCoordinator
{
    bool StartWorker(string workerName);
    bool StopWorker(string workerName);
    IEnumerable<Worker.Data> GetWorkerDatas();
    //Task StartWorkerAsync(string workerName);
    //Task StopWorkerAsync(string workerName);
    // Get Worker Status, lifetime, proccessed items, etc.
    // Multiple workers per type?
}
