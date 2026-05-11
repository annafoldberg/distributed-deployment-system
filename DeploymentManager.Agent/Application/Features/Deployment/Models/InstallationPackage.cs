namespace DeploymentManager.Agent.Application.Features.Deployment.Models;

/// <summary>
/// Represents an installation package retrieved from the API.
/// </summary>
public sealed class InstallationPackage
{
    public Stream Content { get; }
    public string FileName { get; }

    public InstallationPackage(Stream content, string fileName)
    {
        Content = content;
        FileName = fileName;
    }

}