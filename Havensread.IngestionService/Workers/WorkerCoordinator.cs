using Havensread.Connector;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Havensread.IngestionService.Workers;

public sealed class WorkerCoordinator : BackgroundService, IWorkerCoordinator
{
    private readonly IEnumerable<IWorker> _workers;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ILogger<WorkerCoordinator> _logger;
    private readonly ConcurrentDictionary<string, WorkerLifetime> _workerLives;
    private readonly ConcurrentDictionary<string, WorkerState> _workerStates;
    private TaskCompletionSource _startTcs;

    // Delegate to start a worker with the global token
    private Func<string, CancellationToken, bool>? _startWorkerDelegate;

    public WorkerCoordinator(
        IEnumerable<IWorker> workers,
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<WorkerCoordinator> logger)
    {
        _workers = workers;
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;

        _workerLives = new ConcurrentDictionary<string, WorkerLifetime>();
        _workerStates = new ConcurrentDictionary<string, WorkerState>(
            _workers.ToDictionary(worker => worker.Name, _ => WorkerState.Stopped));

        _startTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(async () =>
        {
            foreach (var life in _workerLives.Values)
            {
                life.Cts.Cancel();
            }

            _logger.LogInformation("Worker coordinator shutting down in {Minutes}.", _workerLives.Count);

            await Task.Delay(TimeSpan.FromMinutes(_workerLives.Count));

            _hostApplicationLifetime.StopApplication();
        });

        // Define the delegate to start a worker with the global token
        _startWorkerDelegate = CreateStartWorkerDelegate(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            // Using the completion source to wait for input
            await _startTcs.Task;

            var tasks = _workers
                .Where(worker => _workerStates[worker.Name] == WorkerState.Stopped)
                .Select(worker => ExecuteWorkerAsync(worker, stoppingToken));

            await Task.WhenAll(tasks);

            _startTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }

    public bool StopWorker(string workerName)
    {
        if (_workerLives.TryGetValue(workerName, out var life))
        {
            life.Cts.Cancel();
            return true;
        }

        _logger.LogError("Stop was called but Worker {WorkerName} not found.", workerName);
        return false;
    }

    public bool StartWorker(string workerName, CancellationToken workerToken)
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

    private async Task ExecuteWorkerAsync(IWorker worker, CancellationToken stoppingToken, WorkerLifetime? life = null)
    {
        life ??= _workerLives.GetOrAdd(worker.Name, new WorkerLifetime(worker.Name));

        using var lts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, life.Cts.Token);
        // note:
        // perhaps activity values can be supplied from worker settings - should say what the handler is doing
        using var activity = life.ActivitySource.StartActivity("Worker process", ActivityKind.Internal);

        lts.Token.Register(() =>
        {
            _logger.LogInformation("Worker {WorkerName} was cancelled.", worker.Name);
            _workerStates[worker.Name] = WorkerState.Stopping;
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
            activity?.AddException(ex);
            _logger.LogError(ex, "Error occurred while executing worker {WorkerName}.", worker.Name);
        }
        finally
        {
            await lts.CancelAsync();
            lts.Dispose();
            activity?.Dispose();
            _workerLives.TryRemove(worker.Name, out _);
            _workerStates[worker.Name] = WorkerState.Stopped;
        }
    }

    private Func<string, CancellationToken, bool> CreateStartWorkerDelegate(CancellationToken stoppingToken)
    {
        return (workerName, workerToken) =>
        {
            var worker = _workers.FirstOrDefault(w => w.Name == workerName);
            if (worker is null)
            {
                _logger.LogError("Start was called but Worker {WorkerName} not found.", workerName);
                return false;
            }

            if (_workerLives.ContainsKey(workerName))
            {
                _logger.LogWarning("Worker {WorkerName} is already in process.", workerName);
                return false;
            }

            WorkerLifetime? life = null;
            if (workerToken != stoppingToken && workerToken != CancellationToken.None)
            {
                // Worker token is valid, we wrap it in a token source so that we can keep it
                var cts = CancellationTokenSource.CreateLinkedTokenSource(workerToken, CancellationToken.None);
                life = new WorkerLifetime(workerName, cts);
                _workerLives[workerName] = life;
            }

            _ = Task.Run(async () =>
            {
                await ExecuteWorkerAsync(worker, stoppingToken, life);
            });

            return true;
        };
    }

    public WorkerState GetWorkerState(string workerName)
    {
        var worker = _workers.First(w => w.Name == workerName);
        return _workerStates[worker.Name];
    }

    public IEnumerable<WorkerState> GetWorkerStates()
    {
        foreach (var state in _workerStates.Values)
        {
            yield return state;
        }
    }
}
