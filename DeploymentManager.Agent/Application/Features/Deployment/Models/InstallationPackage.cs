namespace DeploymentManager.Agent.Application.Features.Deployment.Models;

/// <summary>
/// Represents an installation package retrieved from the API.
/// </summary>
public sealed class InstallationPackage
{
    public Stream Content { get; init; } = Stream.Null;
    public string Version { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
}