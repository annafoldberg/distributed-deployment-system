using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using DeploymentManager.Api.Application.Common.Interfaces;
using DeploymentManager.Api.Domain.Entities;
using DeploymentManager.Api.Application.Features.Common.Errors;

namespace DeploymentManager.Api.Application.Features.AuditLogs.Queries;

/// <summary>
/// Handles retrieval of agent audit logs.
/// </summary>
public sealed class GetAgentAuditLogsQueryHandler : IRequestHandler<GetAgentAuditLogsQuery, Result<List<AuditLog>>>
{
    private readonly IDeploymentManagerDbContext _context;
    private readonly ILogger<GetAgentAuditLogsQueryHandler> _logger;

    public GetAgentAuditLogsQueryHandler(IDeploymentManagerDbContext context, ILogger<GetAgentAuditLogsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<AuditLog>>> Handle(GetAgentAuditLogsQuery request, CancellationToken ct)
    {
        var agent = await _context.Agents
            .FirstOrDefaultAsync(a => a.PublicId == request.AgentId, ct);

        if (agent is null)
        {
            _logger.LogWarning("Agent {AgentId} not found in database.", request.AgentId);
            return Result.Fail(new AgentNotFoundError());
        }

        var auditLogs = await _context.AuditLogs
            .Where(l => l.AgentId == agent.Id)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(ct);

        return Result.Ok(auditLogs);
    }
}