namespace DeploymentManager.Agent.Infrastructure.Configuration;

/// <summary>
/// Configuration options for agent identity.
/// </summary>
public sealed class AgentIdentityOptions
{
    public const string SectionName = "AgentIdentity";
    public Guid AgentId { get; init; }
    public string ApiKey { get; init; } = string.Empty;
}