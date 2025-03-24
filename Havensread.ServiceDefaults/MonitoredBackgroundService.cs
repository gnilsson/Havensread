using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Havensread.ServiceDefaults;

public abstract class MonitoredBackgroundService : BackgroundService
{
    private readonly ILogger<MonitoredBackgroundService> _logger;
    private readonly ActivitySource _activitySource;

    protected MonitoredBackgroundService(ILogger<MonitoredBackgroundService> logger)
    {
        _logger = logger;
        _activitySource = new(ActivitySourceName);
    }

    protected abstract string ActivitySourceName { get; }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Service}: Starting.", ServiceName);
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Service}: Stopping.", ServiceName);
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = _activitySource.StartActivity(ServiceName, ActivityKind.Client);

        try
        {
            await RunAsync(stoppingToken);
        }
        catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException)
        {
            _logger.LogInformation("{Service}: Cancellation requested. Stopping the background service.", ServiceName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Service}: An error occurred while processing.", ServiceName);
            activity?.AddException(ex);
        }
        finally
        {
            _logger.LogInformation("{Service}: Completed processing.", ServiceName);
        }
    }

    protected abstract string ServiceName { get; }

    protected abstract Task RunAsync(CancellationToken stoppingToken);
}
