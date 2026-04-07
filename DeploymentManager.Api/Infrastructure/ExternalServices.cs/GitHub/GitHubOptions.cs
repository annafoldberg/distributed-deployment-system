namespace DeploymentManager.Api.Infrastructure.ExternalServices.GitHub;

/// <summary>
/// External GitHub repository options.
/// </summary>
public sealed class GitHubOptions
{
    public string Owner { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
}