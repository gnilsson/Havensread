using System.Diagnostics;

namespace Havensread.IngestionService.Workers;

public interface IWorker
{
    string Name { get; }
    Task ExecuteAsync(CancellationToken cancellationToken);
}
