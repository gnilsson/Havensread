using Havensread.Connector;
using Microsoft.AspNetCore.SignalR.Client;

namespace Havensread.Web.Components.Pages;

public partial class Dashboard
{
    private HubConnection? _hubConnection;
    private List<Worker.Data> _workerDatas = new();
    private CancellationTokenSource _cts = new();
    private bool _error;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:8114/workerHub")
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10) })
                .ConfigureLogging(logging => logging.AddConsole())
                .Build();

            await _hubConnection.StartAsync(_cts.Token);
            await LoadWorkerDatas();
            //StateHasChanged();

            _hubConnection.On(nameof(IWorkerHubClient.SendResultAsync), async (bool result) =>
            {
                if (!result)
                {
                    await InvokeAsync(() =>
                    {
                        _error = true;
                        StateHasChanged();
                    });
                }
            });

            _hubConnection.On(nameof(IWorkerHubClient.SendWorkerDatasAsync), async (IEnumerable<Worker.Data> workerDatas) =>
            {
                await InvokeAsync(() =>
                {
                    _workerDatas = workerDatas.ToList();
                    StateHasChanged();
                });
            });

            _hubConnection.Reconnected += _ =>
            {
                Logger.LogInformation("Reconnected to SignalR hub.");
                return Task.CompletedTask;
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to initialize SignalR connection");
        }
    }

    private async Task LoadWorkerDatas()
    {
        if (_hubConnection is not null)
        {
            var workerDatas = await _hubConnection.InvokeAsync<IEnumerable<Worker.Data>>(WorkerHub.ServerMethodName.GetWorkerDatas);
            _workerDatas = workerDatas.ToList();
        }
    }

    private async Task StartWorkers() =>
        await SafeHubInvokeAsync(WorkerHub.ServerMethodName.StartWorkers);

    private async Task StartWorker(string workerName) =>
        await SafeHubInvokeAsync(WorkerHub.ServerMethodName.StartWorker, workerName);

    private async Task StopWorker(string workerName) =>
        await SafeHubInvokeAsync(WorkerHub.ServerMethodName.StopWorker, workerName);

    private async Task SafeHubInvokeAsync(string methodName, string? args = null)
    {
        try
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.InvokeAsync(methodName, args);
               // await LoadWorkerDatas(); // Refresh data after action
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error invoking {Method}", methodName);
        }
    }

    private string GetStatusBadgeClass(Worker.State state) =>
        state switch
        {
            Worker.State.Running => "bg-success",
            Worker.State.Stopped => "bg-secondary",
            _ => "bg-warning"
        };

    private string GetButtonClass(Worker.State state, string action) => action switch
    {
        "start" => state == Worker.State.Running
            ? "bg-gray-300 text-gray-600 cursor-not-allowed"
            : "bg-haven-green hover:bg-green-600 text-white",
        "stop" => state != Worker.State.Running
            ? "bg-gray-300 text-gray-600 cursor-not-allowed"
            : "bg-haven-red hover:bg-red-600 text-white",
        _ => ""
    };

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
        _cts.Dispose();
    }
}
