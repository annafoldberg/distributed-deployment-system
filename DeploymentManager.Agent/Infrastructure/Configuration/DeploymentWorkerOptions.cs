namespace DeploymentManager.Agent.Infrastructure.Configuration;

/// <summary>
/// Configuration options for deployment worker.
/// </summary>
public sealed class DeploymentWorkerOptions
{
    public const string SectionName = "DeploymentWorker";
    public int IntervalSeconds { get; init; }
}