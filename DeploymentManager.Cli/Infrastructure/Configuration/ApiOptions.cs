namespace DeploymentManager.Cli.Infrastructure.Configuration;

/// <summary>
/// Configuration options for API.
/// </summary>
public sealed class ApiOptions
{
    public const string SectionName = "Api";
    public string BaseUrl { get; init; } = string.Empty;
}