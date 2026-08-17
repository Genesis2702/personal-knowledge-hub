using System.Collections.Concurrent;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;

namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure;

public sealed class RecordingBackgroundJobClient : IBackgroundJobClient, IResettableBackgroundJobClient
{
    private readonly ConcurrentQueue<Job> _jobs = new();

    public IReadOnlyCollection<Job> Jobs => _jobs.ToArray();
    
    public string Create(Job job, IState state)
    {
        _jobs.Enqueue(job);
        return Guid.NewGuid().ToString();
    }

    public bool ChangeState(string jobId, IState state, string expectedState)
    {
        return true;
    }

    public void Reset()
    {
        _jobs.Clear();
    }
}