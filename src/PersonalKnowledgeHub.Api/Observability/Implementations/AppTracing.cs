using System.Diagnostics;

namespace PersonalKnowledgeHub.Observability.Implementations;

public static class AppTracing
{
    public const string ServiceName = "PersonalKnowledgeHub";
    public static readonly ActivitySource ActivitySource = new(ServiceName);
}