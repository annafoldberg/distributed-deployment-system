namespace DeploymentManager.Api.Infrastructure.Configuration;

/// <summary>
/// Configuration options for database connection.
/// </summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "Database";
    public string Host { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string User { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}