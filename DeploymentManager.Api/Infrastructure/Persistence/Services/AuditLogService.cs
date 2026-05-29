using DeploymentManager.Api.Application.Common.Interfaces;
using DeploymentManager.Api.Domain.Entities;
using DeploymentManager.Api.Domain.Enums;

namespace DeploymentManager.Api.Infrastructure.Persistence.Services;

/// <summary>
/// Inserts audit log entries to database.
/// </summary>
public sealed class AuditLogService : IAuditLogService
{
    private readonly IDeploymentManagerDbContext _context;

    public AuditLogService(IDeploymentManagerDbContext context)
    {
        _context = context;
    }

    public async Task AddCustomerLogAsync(int customerId, AuditLogLevel level, string message, CancellationToken ct)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            CreatedAt = DateTimeOffset.UtcNow,
            Level = level,
            Message = message,
            CustomerId = customerId
        });

        await _context.SaveChangesAsync(ct);
    }
    
    public async Task AddAgentLogAsync(int agentId, AuditLogLevel level, string message, CancellationToken ct)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            CreatedAt = DateTimeOffset.UtcNow,
            Level = level,
            Message = message,
            AgentId = agentId
        });

        await _context.SaveChangesAsync(ct);
    }
}