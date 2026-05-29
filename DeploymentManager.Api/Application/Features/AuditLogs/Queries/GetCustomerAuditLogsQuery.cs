using MediatR;
using FluentResults;
using DeploymentManager.Api.Domain.Entities;

namespace DeploymentManager.Api.Application.Features.AuditLogs.Queries;

/// <summary>
/// Query for retrieving customer audit logs.
/// </summary>
public sealed record GetCustomerAuditLogsQuery(Guid CustomerId) : IRequest<Result<List<AuditLog>>>;