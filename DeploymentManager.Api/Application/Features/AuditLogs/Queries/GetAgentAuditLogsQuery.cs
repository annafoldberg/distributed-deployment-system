using MediatR;
using FluentResults;
using DeploymentManager.Api.Domain.Entities;

namespace DeploymentManager.Api.Application.Features.AuditLogs.Queries;

/// <summary>
/// Query for retrieving agent audit logs.
/// </summary>
public sealed record GetAgentAuditLogsQuery(Guid AgentId) : IRequest<Result<List<AuditLog>>>;