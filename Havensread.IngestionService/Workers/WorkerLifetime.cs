using System.Diagnostics;

namespace Havensread.IngestionService.Workers;

public sealed class WorkerLifetime
{
    public WorkerLifetime(string workerName)
    {
        Cts = new CancellationTokenSource();
        ActivitySource = new ActivitySource(workerName);
    }

    public WorkerLifetime(string workerName, CancellationTokenSource cts)
    {
        Cts = cts;
        ActivitySource = new ActivitySource(workerName);
    }

    public CancellationTokenSource Cts { get; }
    public ActivitySource ActivitySource { get; }
}
