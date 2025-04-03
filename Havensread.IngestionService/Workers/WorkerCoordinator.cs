using Havensread.Connector;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading.Channels;

namespace Havensread.IngestionService.Workers;

public sealed class WorkerCoordinator : BackgroundService, IWorkerCoordinator
{
    private abstract record Command(string WorkerName)
    {
        public record Start(string WorkerName) : Command(WorkerName);
        public record Stop(string WorkerName) : Command(WorkerName);
    }

    private readonly static Channel<Command> s_commandChannel = Channel.CreateUnbounded<Command>();
    private readonly ImmutableArray<IWorker> _workers;
    private readonly ConcurrentDictionary<string, WorkerLifetime> _workerLives = new();
    //  private readonly ConcurrentDictionary<string, Worker.State> _workerStates;
    private readonly ILogger<WorkerCoordinator> _logger;
    private readonly IHubContext<WorkerHub, IWorkerHubClient> _hubContext;

    public WorkerCoordinator(IEnumerable<IWorker> workers, IHubContext<WorkerHub, IWorkerHubClient> hubContext, ILogger<WorkerCoordinator> logger)
    {
        _workers = workers.ToImmutableArray();
        //    _workerStates = new(_workers.ToDictionary(w => w.Name, _ => Worker.State.Stopped));
        _workerLives = new(_workers.ToDictionary(w => w.Name, w => new WorkerLifetime(w.Name)));
        _hubContext = hubContext;
        _logger = logger;
    }

    public bool StartWorker(string workerName) =>
        s_commandChannel.Writer.TryWrite(new Command.Start(workerName));

    public bool StopWorker(string workerName) =>
        s_commandChannel.Writer.TryWrite(new Command.Stop(workerName));

    //public async Task StartWorkerAsync(string workerName)
    //{
    //    if (s_commandChannel.Writer.TryWrite(new Command.Start(workerName)))
    //    {
    //        await _hubContext.Clients.All.SendWorkerDatasAsync(GetWorkerDatas());
    //    }
    //}

    //public async Task StopWorkerAsync(string workerName)
    //{
    //    if (s_commandChannel.Writer.TryWrite(new Command.Stop(workerName)))
    //    {
    //        await _hubContext.Clients.All.SendWorkerDatasAsync(GetWorkerDatas());
    //    }
    //}

    public IEnumerable<Worker.Data> GetWorkerDatas()
    {
        foreach (var workerLife in _workerLives.Values)
        {
            yield return new Worker.Data(
                workerLife.Name,
                workerLife.State,
                workerLife.LastCommandTime);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var command in s_commandChannel.Reader.ReadAllAsync(stoppingToken))
        {
            var status = command switch
            {
                Command.Start _ => StartWorkerImpl(command.WorkerName),
                Command.Stop _ => StopWorkerImpl(command.WorkerName),
                _ => false
            };

            var task = status
              ? _hubContext.Clients.All.SendWorkerDatasAsync(GetWorkerDatas())
              : _hubContext.Clients.All.SendResultAsync(false);
            await task;

            _logger.LogInformation("Command {Command} for worker {Worker} with approved: {Approved}", command.GetType().Name, command.WorkerName, status);
        }
    }

    private bool StartWorkerImpl(string workerName)
    {
        //if (!_workers.Any(w => w.Name == workerName))
        if (!_workerLives.TryGetValue(workerName, out var endedLife))
        {
            _logger.LogWarning("Worker {WorkerName} not found.", workerName);
            return false;
        }

        //if (_workerLives.TryGetValue(workerName, out var life) && life.State is Worker.State.Running)
        //if (_workerLives.ContainsKey(workerName) || _workerStates[workerName] == Worker.State.Running)
        if (endedLife.State is Worker.State.Running)
        {
            _logger.LogWarning("Worker {WorkerName} is already running.", workerName);
            return false;
        }

        var newLife = endedLife.State is Worker.State.Initialized ? endedLife : new WorkerLifetime(workerName);
        newLife.Start();

        if (!_workerLives.TryUpdate(workerName, newLife, endedLife))
        {
            _logger.LogWarning("Worker {WorkerName} could not start.", workerName);
            return false;
        }

        _ = ExecuteWorkerAsync(workerName, newLife)
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    _logger.LogError(t.Exception, "Worker {WorkerName} crashed", workerName);
                }
            }, TaskContinuationOptions.OnlyOnFaulted);

        return true;
    }

    private async Task ExecuteWorkerAsync(string workerName, WorkerLifetime lifetime)
    {
        using (lifetime)
        using (var activity = lifetime.ActivitySource.StartActivity(workerName))
        {
            var worker = _workers.First(w => w.Name == workerName);

            try
            {
                await worker.ExecuteAsync(lifetime.Cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Worker {WorkerName} stopped gracefully.", workerName);
            }
            catch (Exception ex)
            {
                activity?.AddException(ex);
                activity?.SetStatus(ActivityStatusCode.Error);
                _logger.LogError(ex, "Worker {WorkerName} failed.", workerName);
            }
            finally
            {
                // The lifetime handles the disposal.
                //_workerLives[workerName].State = Worker.State.Stopped;
            }
        }
    }

    // when I stop will finally trigger?
    private bool StopWorkerImpl(string workerName)
    {
        if (_workerLives.TryGetValue(workerName, out var lifetime))
        {
            lifetime.Dispose();
            return true;
        }

        return false;
    }
}
