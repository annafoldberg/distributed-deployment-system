
using DeploymentManager.Agent.Application.Features.Deployment.Models;

namespace DeploymentManager.Agent.Application.Features.Deployment.Interfaces;

/// <summary>
/// Interface for Deployment Manager API requests.
/// </summary>
public interface IDeploymentManagerApiClient
{
    /// <summary>
    /// Retrieves installation packages from Deployment Manager API.
    /// </summary>
    Task<InstallationPackage?> GetInstallationPackageAsync(CancellationToken ct);

    /// <summary>
    /// Reports the result of an installation attempt to Deployment Manager API.
    /// </summary>
    Task ReportInstallationResultAsync(bool succeeded, string? installedVersion, string? errorMessage, CancellationToken ct);
}