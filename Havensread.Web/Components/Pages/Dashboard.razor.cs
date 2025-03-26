using Havensread.Connector;
using Havensread.Connector.Messages;
using MassTransit;
using Microsoft.AspNetCore.SignalR.Client;

namespace Havensread.Web.Components.Pages;

public partial class Dashboard
{
    private HubConnection? _hubConnection;
    //private List<WorkerStatus> workerStatuses = new();

    protected override async Task OnInitializedAsync()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:8114/workerHub")
            .Build();

        // hubConnection.On<IEnumerable<WorkerStatus>>("ReceiveWorkerStatuses", (statuses) =>
        // {
        //     workerStatuses = statuses.ToList();
        //     StateHasChanged();
        // });

        await _hubConnection.StartAsync();

        _hubConnection.On(nameof(IWorkerHub.SendResultAsync), (bool result) =>
        {
            Console.WriteLine(result);
        });

        //await LoadWorkerStatuses();
    }

    //private async Task Send()
    //{
    //    if (_hubConnection is not null)
    //    {
    //        await Bus.Publish(new WorkerMessage { WorkerName = WorkerNames.BookIngestionWorker });
    //    }
    //}

    private async Task StartWorkers()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.InvokeAsync(WorkerHub.ServerMethodName.StartWorkers);
            //await LoadWorkerStatuses();
        }
    }

    private async Task StartWorker(string workerName)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.InvokeAsync(WorkerHub.ServerMethodName.StartWorker, workerName);
        }
    }

    private async Task StopWorker(string workerName)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.InvokeAsync(WorkerHub.ServerMethodName.StopWorker, workerName);
            //await LoadWorkerStatuses();
        }
    }

    private async Task StopCoordinator()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.InvokeAsync(WorkerHub.ServerMethodName.StopCoordinator);
            //await LoadWorkerStatuses();
        }
    }
    // private async Task LoadWorkerStatuses()
    // {
    //     if (hubConnection is not null)
    //     {
    //         var statuses = await hubConnection.InvokeAsync<IEnumerable<WorkerStatus>>("GetWorkerStatuses");
    //         workerStatuses = statuses.ToList();
    //     }
    // }

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
