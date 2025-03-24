namespace Havensread.MigrationService.Workers;

public sealed class MigrationWorkerCoordinator : BackgroundService
{
    private readonly AppMigrationWorker _appWorker;
    private readonly IngestionMigrationWorker _ingestionWorker;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ILogger<MigrationWorkerCoordinator> _logger;

    public MigrationWorkerCoordinator(
        AppMigrationWorker worker,
        IngestionMigrationWorker ingestionWorker,
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<MigrationWorkerCoordinator> logger)
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
        finally
        {
            _logger.LogInformation("Migration worker coordinator shutting down.");

            _hostApplicationLifetime.StopApplication();
        }
    }
}
