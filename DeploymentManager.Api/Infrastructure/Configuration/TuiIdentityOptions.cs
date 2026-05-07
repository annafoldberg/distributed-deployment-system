namespace DeploymentManager.Api.Infrastructure.Configuration;

/// <summary>
/// Configuration options for TUI identity.
/// </summary>
public sealed class TuiIdentityOptions
{
    public const string SectionName = "TuiIdentity";
    public string ApiKeyHash { get; init; } = string.Empty;
}