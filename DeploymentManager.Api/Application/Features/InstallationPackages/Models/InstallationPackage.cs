namespace DeploymentManager.Api.Application.Features.InstallationPackages.Models;

/// <summary>
/// Downloadable installation package.
/// </summary>
public class InstallationPackage
{
    public Stream Content { get; }
    public string ContentType { get; }
    public string FileName { get; }

    public InstallationPackage(Stream content, string contentType, string fileName)
    {
        Content = content;
        ContentType = contentType;
        FileName = fileName;
    }
}