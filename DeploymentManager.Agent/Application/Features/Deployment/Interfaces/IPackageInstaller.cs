using DeploymentManager.Agent.Application.Features.Deployment.Models;
using DeploymentManager.Agent.Application.Features.Deployment.Results;

namespace DeploymentManager.Agent.Application.Features.Deployment.Interfaces;

/// <summary>
/// Installs packages.
/// </summary>
public interface IPackageInstaller
{
    /// <summary>
    /// Installs the specified package.
    /// </summary>
    /// <param name="package">Package to install.</param>
    /// <param name="ct">Cancellation token for installation.</param>
    /// <returns>Result of the installation.</returns>
    Task<InstallationResult> InstallPackageAsync(InstallationPackage package, CancellationToken ct);
}