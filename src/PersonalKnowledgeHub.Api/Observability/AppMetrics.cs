using System.Diagnostics.Metrics;

namespace PersonalKnowledgeHub.Observability;

public class AppMetrics
{
    private readonly Counter<int> _emailSendFailureCounter;
    private readonly Counter<int> _loginFailureCounter;

    public AppMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("PersonalKnowledgeHub");
        _emailSendFailureCounter = meter.CreateCounter<int>("personal_knowledge_hub.email.send.failure");
        _loginFailureCounter = meter.CreateCounter<int>("personal_knowledge_hub.login.failure");
    }

    public void EmailSendFailed()
    {
        _emailSendFailureCounter.Add(1);
    }
    
    public void LoginFailed()
    {
        _loginFailureCounter.Add(1);
    }
}