using System.Diagnostics;

namespace PersonalKnowledgeHub.Observability;

public static class AppTracing
{
    public const string ServiceName = "PersonalKnowledgeHub";
    public static readonly ActivitySource ActivitySource = new(ServiceName);
}