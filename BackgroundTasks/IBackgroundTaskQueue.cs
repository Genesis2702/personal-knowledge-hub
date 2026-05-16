namespace PersonalKnowledgeHub.BackgroundTasks;

public interface IBackgroundTaskQueue
{
    public ValueTask Enqueue(Func<CancellationToken, ValueTask> workItem);
    public ValueTask<Func<CancellationToken, ValueTask>> Dequeue(CancellationToken cancellationToken);
}