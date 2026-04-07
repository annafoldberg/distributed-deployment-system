using MediatR;
using FluentResults;
using DeploymentManager.Api.Application.Features.InstallationPackages.Models;

namespace DeploymentManager.Api.Application.Features.InstallationPackages.Queries;

/// <summary>
/// Query for retrieving the installation package for an agent.
/// </summary>
public record GetInstallationPackageQuery(Guid AgentId) : IRequest<Result<InstallationPackage>>;