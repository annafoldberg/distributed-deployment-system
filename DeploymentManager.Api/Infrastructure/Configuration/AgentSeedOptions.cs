namespace DeploymentManager.Api.Infrastructure.Configuration;

/// <summary>
/// Seed configuration options for agent entities.
/// </summary>
public sealed class AgentSeedOptions
{
    public const string SectionName = "AgentSeeding";
    public List<SeededAgent> Agents { get; init; } = new();
}

public sealed class SeededAgent
{
    public Guid PublicId { get; init; }
    public string ApiKey { get; init; } = string.Empty;
    public string Platform { get; init; } = string.Empty;
}