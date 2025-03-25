namespace Havensread.Connector;

public interface IWorkerHub
{
    Task SendResultAsync(bool result);
}
