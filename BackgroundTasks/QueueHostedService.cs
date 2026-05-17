namespace PersonalKnowledgeHub.BackgroundTasks;

public class QueueHostedService : BackgroundService
{
    public IBackgroundTaskQueue TaskQueue { get; }

    public QueueHostedService(IBackgroundTaskQueue taskQueue)
    {
        TaskQueue = taskQueue;
    }

    private async Task BackgroundProcessing(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await TaskQueue.Dequeue(stoppingToken);
            try
            {
                await workItem(stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await BackgroundProcessing(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        await base.StopAsync(stoppingToken);
    }
}