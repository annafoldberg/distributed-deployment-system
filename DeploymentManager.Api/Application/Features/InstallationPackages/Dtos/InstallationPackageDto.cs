namespace DeploymentManager.Api.Application.Features.InstallationPackages.Dtos;

/// <summary>
/// Data transfer object for an installation package.
/// </summary>
public sealed class InstallationPackageDto
{
    public Stream Content { get; init; }
    public string ContentType { get; init; }
    public string FileName { get; init; }
}