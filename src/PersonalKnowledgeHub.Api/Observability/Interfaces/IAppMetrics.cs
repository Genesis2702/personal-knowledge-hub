namespace PersonalKnowledgeHub.Observability.Interfaces;

public interface IAppMetrics
{
    public void EmailSendFailed();
    public void LoginFailed();
}