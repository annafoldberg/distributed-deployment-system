namespace DeploymentManager.Cli.Infrastructure.Configuration;

/// <summary>
/// Configuration options for CLI identity.
/// </summary>
public sealed class CliIdentityOptions
{
    public const string SectionName = "CliIdentity";
    public string ApiKey { get; init; } = string.Empty;
}