namespace DeploymentManager.Api.Infrastructure.Configuration;

/// <summary>
/// Configuration options for external GitHub repository.
/// </summary>
public sealed class GitHubOptions
{
    public const string SectionName = "GitHub";
    public string Token { get; init; } = string.Empty;
    public string Owner { get; init; } = string.Empty;
    public string Repository { get; init; } = string.Empty;
}