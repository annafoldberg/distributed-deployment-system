namespace DeploymentManager.Agent.Infrastructure.Configuration;

/// <summary>
/// Configuration options for installation.
/// </summary>
public sealed class InstallationOptions
{
    public const string SectionName = "File";
    public string AppDirectory { get; init; } = string.Empty;
    public string SubDirectory { get; init; } = string.Empty;
    public string Root { get; set; } = string.Empty;
    public string InstallDirectory => Path.Combine(Root, SubDirectory, AppDirectory);
}