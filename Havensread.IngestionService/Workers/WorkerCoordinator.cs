using Havensread.Connector;
using System.Collections.Concurrent;

namespace Havensread.IngestionService.Workers;

public sealed class WorkerCoordinator : BackgroundService, IWorkerCoordinator
{
    private enum WorkerState
    {
        Running,
        Stopped,
    }

    private readonly IEnumerable<IWorker> _workers;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ILogger<WorkerCoordinator> _logger;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _workerCts;
    private readonly ConcurrentDictionary<string, WorkerState> _workerStates;
    private TaskCompletionSource _startTcs;

    // Delegate to start a worker with the global token
    private Func<string, CancellationToken, Task<bool>>? _startWorkerDelegate;

    public WorkerCoordinator(
        IEnumerable<IWorker> workers,
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<WorkerCoordinator> logger)
    {
        _workers = workers;
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;
        _workerCts = new ConcurrentDictionary<string, CancellationTokenSource>(
            _workers.ToDictionary(workers => workers.Name, _ => new CancellationTokenSource()));
        _workerStates = new ConcurrentDictionary<string, WorkerState>(
            _workers.ToDictionary(workers => workers.Name, _ => WorkerState.Stopped));
        _startTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(async () =>
        {
            foreach (var cts in _workerCts.Values)
            {
                cts.Cancel();
            }

            _logger.LogInformation("Worker coordinator shutting down in {Minutes}.", _workerCts.Count);

            await Task.Delay(TimeSpan.FromMinutes(_workerCts.Count));

            _hostApplicationLifetime.StopApplication();
        });

        // Define the delegate to start a worker with the global token
        _startWorkerDelegate = CreateStartWorkerDelegate(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await _startTcs.Task;

            _startTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var tasks = _workers
                .Where(worker => _workerStates[worker.Name] == WorkerState.Stopped)
                .Select(worker => ExecuteWorkerAsync(worker, stoppingToken));

            await Task.WhenAll(tasks);
        }
    }


    public bool StopWorker(string workerName)
    {
        if (_workerCts.TryGetValue(workerName, out var cts))
        {
            cts.Cancel();
            return true;
        }

        _logger.LogError("Stop was called but Worker {WorkerName} not found.", workerName);
        return false;
    }

    public Task<bool> StartWorkerAsync(string workerName, CancellationToken workerToken)
    {
        ArgumentNullException.ThrowIfNull(_startWorkerDelegate, nameof(_startWorkerDelegate));

        return _startWorkerDelegate(workerName, workerToken);
    }

    public bool StartWorkers()
    {
        if (_startTcs.TrySetResult())
        {
            _logger.LogInformation("Restart signal sent to all workers.");
            return true;
        }
        return false;
    }

    public Task StopCoordinatorAsync()
    {
        return base.StopAsync(CancellationToken.None);
    }

    private async Task ExecuteWorkerAsync(IWorker worker, CancellationToken stoppingToken, CancellationToken? workerToken = null)
    {
        workerToken ??= _workerCts.GetOrAdd(worker.Name, new CancellationTokenSource()).Token;

        using var lts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, workerToken.Value);

        lts.Token.Register(() =>
        {
            _logger.LogInformation("Worker {WorkerName} was cancelled.", worker.Name);
            // Callback to canceller?
        });

        try
        {
            _workerStates[worker.Name] = WorkerState.Running;

            await worker.ExecuteAsync(lts.Token);
        }
        catch (OperationCanceledException)
        { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while executing worker {WorkerName}.", worker.Name);
            // Manual restart required
        }
        finally
        {
            _workerCts.TryRemove(worker.Name, out _);
            lts.Dispose();
            _workerStates[worker.Name] = WorkerState.Stopped;
        }
    }

    private Func<string, CancellationToken, Task<bool>> CreateStartWorkerDelegate(CancellationToken stoppingToken)
    {
        return async (workerName, workerToken) =>
        {
            var worker = _workers.FirstOrDefault(w => w.Name == workerName);
            if (worker is null)
            {
                _logger.LogError("Start was called but Worker {WorkerName} not found.", workerName);
                return false;
            }

            if (_workerStates.TryGetValue(workerName, out var state) && state is WorkerState.Running)
            {
                _logger.LogWarning("Worker {WorkerName} is already running.", workerName);
                return false;
            }

            if (workerToken == stoppingToken || workerToken == CancellationToken.None)
            {
                await ExecuteWorkerAsync(worker, stoppingToken);
                return false;
            }

            var cts = CancellationTokenSource.CreateLinkedTokenSource(workerToken, CancellationToken.None);
            _workerCts[workerName] = cts;

            await ExecuteWorkerAsync(worker, stoppingToken, cts.Token);
            return true;
        };
    }
}
