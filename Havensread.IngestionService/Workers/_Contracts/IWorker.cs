namespace Havensread.IngestionService.Workers;

public interface IWorker
{
    string Name { get; }
    Task ExecuteAsync(CancellationToken stoppingToken);
}
