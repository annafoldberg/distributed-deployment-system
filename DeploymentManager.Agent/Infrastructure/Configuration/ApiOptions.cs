namespace DeploymentManager.Agent.Infrastructure.Configuration;

/// <summary>
/// Configuration options for api.
/// </summary>
public sealed class ApiOptions
{
    public const string SectionName = "Api";
    public string BaseUrl { get; init; } = string.Empty;
}