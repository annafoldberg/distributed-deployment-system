namespace DeploymentManager.Api.Domain.Entities;

/// <summary>
/// Represents a deployable release of the managed software.
/// </summary>
public class Release
{
    public int Id { get; set; }
    public string Version { get; set; } = string.Empty;
}