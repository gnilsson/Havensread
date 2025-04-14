namespace Havensread.Connector.Messages;

public sealed class WorkerMessage
{
    public required string WorkerName { get; init; }
    public int? BatchSize { get; init; }
}
