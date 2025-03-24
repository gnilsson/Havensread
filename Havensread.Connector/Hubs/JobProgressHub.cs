using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Havensread.Connector;

public sealed class JobProgressHub : Hub
{
    public async Task SendProgress(string jobId, int progress)
    {
        await Clients.All.SendAsync("ReceiveProgress", jobId, progress);
    }

    public async Task SendStatus(string jobId, string status)
    {
        await Clients.All.SendAsync("ReceiveStatus", jobId, status);
    }



}

public sealed class ProcessingCommandConsumer : IConsumer
{

}
