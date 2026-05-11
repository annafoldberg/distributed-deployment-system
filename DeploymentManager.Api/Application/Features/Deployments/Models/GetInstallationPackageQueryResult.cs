namespace DeploymentManager.Api.Application.Features.Deployments.Models;

/// <summary>
/// Represents result of installation package query.
/// </summary>
public sealed class GetInstallationPackageQueryResult
{
    public InstallationPackageStatus Status { get; init; }
    public InstallationPackageDto? InstallationPackage { get; init; }
}