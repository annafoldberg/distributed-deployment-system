namespace DeploymentManager.Api.Application.Features.Deployments.Models;

/// <summary>
/// Status of an installation package query result.
/// </summary>
public enum InstallationPackageStatus
{
    Available,
    NoUpdateRequired,
    DesiredReleaseNotSet
}