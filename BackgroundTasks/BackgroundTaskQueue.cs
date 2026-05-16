using System.Threading.Channels;

namespace PersonalKnowledgeHub.BackgroundTasks;

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<CancellationToken, ValueTask>> _queue;

    public BackgroundTaskQueue(int capacity)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };
        _queue = Channel.CreateBounded<Func<CancellationToken, ValueTask>>(options);
    }
    
    public ValueTask Enqueue(Func<CancellationToken, ValueTask> workItem)
    {
        return _queue.Writer.WriteAsync(workItem);
    }
    
    public ValueTask<Func<CancellationToken, ValueTask>> Dequeue(CancellationToken cancellationToken)
    {
        return _queue.Reader.ReadAsync(cancellationToken);
    }
}