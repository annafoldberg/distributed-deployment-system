namespace DeploymentManager.Api.Presentation.Contracts;

/// <summary>
/// Data transfer object for an installation result.
/// </summary>
public sealed class InstallationResultDto
{
    public bool Succeeded { get; init; }
    public string? InstalledVersion { get; init; }
    public string? ErrorMessage { get; init; }
}