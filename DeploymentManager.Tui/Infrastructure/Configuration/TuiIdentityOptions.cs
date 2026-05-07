namespace DeploymentManager.Tui.Infrastructure.Configuration;

/// <summary>
/// Configuration options for TUI identity.
/// </summary>
public sealed class TuiIdentityOptions
{
    public const string SectionName = "TuiIdentity";
    public string ApiKey { get; init; } = string.Empty;
}