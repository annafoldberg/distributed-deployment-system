using MediatR;
using FluentResults;
using DeploymentManager.Api.Application.Features.InstallationPackages.Dtos;

namespace DeploymentManager.Api.Application.Features.InstallationPackages.Queries;

/// <summary>
/// Query for retrieving the installation package for an agent.
/// </summary>
public sealed record GetInstallationPackageQuery(Guid AgentId) : IRequest<Result<InstallationPackageDto>>;