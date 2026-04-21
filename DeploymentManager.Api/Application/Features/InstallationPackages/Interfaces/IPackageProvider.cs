using DeploymentManager.Api.Application.Features.InstallationPackages.Dtos;

namespace DeploymentManager.Api.Application.Features.InstallationPackages.Interfaces;

/// <summary>
/// Provides installation packages.
/// </summary>
public interface IPackageProvider
{
    /// <summary>
    /// Retrieves the installation package matching the specified platform and version.
    /// </summary>
    /// <param name="platform">Platform identifier (e.g. win-x64, osx-arm64).</param>
    /// <param name="version">Version (e.g. 1.0.0).</param>
    /// <returns>The installation package if found, otherwise <c>null</c>.</returns>
    Task<InstallationPackageDto?> FetchPackageAsync(string platform, string version, CancellationToken ct);
}