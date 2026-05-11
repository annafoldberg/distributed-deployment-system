using DeploymentManager.Agent.Application.Features.Deployment.Models;

namespace DeploymentManager.Agent.Application.Features.Deployment.Results;

/// <summary>
/// Represents the result of installation package retrieval from the API.
/// </summary>
public sealed class InstallationPackageRetrievalResult
{
    public PackageRetrievalStatus Status { get; init; }
    public InstallationPackage? InstallationPackage { get; init; }
}