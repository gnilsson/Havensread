namespace Havensread.IngestionService.Workers;

public sealed class WorkerDefaults
{
    public const int ChunkSize = 10;
    public const int BatchSize = ChunkSize * 5;
    public const int MaxFailedCount = ChunkSize - 3;
}
