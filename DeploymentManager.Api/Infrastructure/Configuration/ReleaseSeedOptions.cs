namespace DeploymentManager.Api.Infrastructure.Configuration;

/// <summary>
/// Seed configuration options for release entities.
/// </summary>
public sealed class ReleaseSeedOptions
{
    public const string SectionName = "ReleaseSeeding";
    public List<SeededRelease> Releases { get; init; } = [];
}

public sealed class SeededRelease
{
    public string Version { get; init; } = string.Empty;
}