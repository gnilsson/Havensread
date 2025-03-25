namespace Havensread.Connector;

public interface IWorkerCoordinatorSentry
{
    WorkerState GetWorkerState(string workerName);
    IEnumerable<WorkerState> GetWorkerStates();
}
