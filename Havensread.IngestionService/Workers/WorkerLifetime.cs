using Havensread.Connector;
using System.Diagnostics;

namespace Havensread.IngestionService.Workers;

internal sealed class WorkerLifetime : IDisposable
{
    public string Name { get; }
    public CancellationTokenSource Cts { get; }
    public ActivitySource ActivitySource { get; }
    public Worker.State State { get; private set; }
    public DateTimeOffset LastCommandTime { get; private set; }

    public WorkerLifetime(string name)
    {
        Name = name;
        ActivitySource = new ActivitySource(name);
        Cts = new CancellationTokenSource();
    }

    public void DeclareStarted()
    {
        State = Worker.State.Running;
        LastCommandTime = DateTimeOffset.UtcNow;
    }

    public void Dispose()
    {
        Cts.Cancel();
        Cts.Dispose();
        ActivitySource.Dispose();
        State = Worker.State.Stopped;
        LastCommandTime = DateTimeOffset.UtcNow;
    }
}
