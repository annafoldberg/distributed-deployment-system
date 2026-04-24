namespace DeploymentManager.Api.Application.Features.InstallationPackages.Dtos;

/// <summary>
/// Data transfer object for an installation package.
/// </summary>
public sealed class InstallationPackageDto
{
    public Stream Content { get; }
    public string ContentType { get; }
    public string FileName { get; }

    public InstallationPackageDto(Stream content, string contentType, string fileName)
    {
        Content = content;
        ContentType = contentType;
        FileName = fileName;
    }
}