namespace Havensread.Connector;

public interface IWorkerHubClient
{
    Task SendResultAsync(bool result);
    Task SendWorkerDatasAsync(IEnumerable<Worker.Data> datas);
}
