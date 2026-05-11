using MediatR;
using FluentResults;
using DeploymentManager.Api.Application.Features.Deployments.Models;

namespace DeploymentManager.Api.Application.Features.Deployments.Queries;

/// <summary>
/// Query for retrieving the installation package for an agent.
/// </summary>
public sealed record GetInstallationPackageQuery(Guid AgentId) : IRequest<Result<GetInstallationPackageQueryResult>>;