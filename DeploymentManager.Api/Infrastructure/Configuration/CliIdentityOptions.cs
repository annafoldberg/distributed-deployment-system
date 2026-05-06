namespace DeploymentManager.Api.Infrastructure.Configuration;

/// <summary>
/// Configuration options for CLI identity.
/// </summary>
public sealed class CliIdentityOptions
{
    public const string SectionName = "CliIdentity";
    public string ApiKeyHash { get; init; } = string.Empty;
}