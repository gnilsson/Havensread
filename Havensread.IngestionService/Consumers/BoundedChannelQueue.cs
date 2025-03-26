//using System.Runtime.CompilerServices;
//using System.Threading.Channels;

//namespace Havensread.IngestionService;

//public sealed class BoundedChannelQueue<T>
//{
//    private readonly Channel<T> _channel;

//    public BoundedChannelQueue()
//    {
//        var options = new BoundedChannelOptions(100)
//        {
//            FullMode = BoundedChannelFullMode.Wait
//        };
//        _channel = Channel.CreateBounded<T>(options);
//    }

//    public async Task EnqueueAsync(IEnumerable<T> messages)
//    {
//        foreach (var message in messages)
//        {
//            await _channel.Writer.WriteAsync(message);
//        }
//        _channel.Writer.Complete();
//    }

//    public async IAsyncEnumerable<T> DequeueAllAsync([EnumeratorCancellation] CancellationToken cancellationToken)
//    {
//        await foreach (var message in _channel.Reader.ReadAllAsync(cancellationToken))
//        {
//            yield return message;
//        }
//    }
//}
