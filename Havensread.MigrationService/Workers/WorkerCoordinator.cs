namespace Havensread.MigrationService.Workers;

public sealed class WorkerCoordinator : BackgroundService
{
    private readonly AppWorker _appWorker;
    private readonly IngestionWorker _ingestionWorker;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ILogger<WorkerCoordinator> _logger;

    public WorkerCoordinator(
        AppWorker worker,
        IngestionWorker ingestionWorker,
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<WorkerCoordinator> logger)
    {
        _appWorker = worker;
        _ingestionWorker = ingestionWorker;
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(() => _hostApplicationLifetime.StopApplication());

        try
        {
            await _ingestionWorker.ExecuteAsync(stoppingToken);
            await _appWorker.ExecuteAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while executing workers.");
        }

        _hostApplicationLifetime.StopApplication();
    }
}
