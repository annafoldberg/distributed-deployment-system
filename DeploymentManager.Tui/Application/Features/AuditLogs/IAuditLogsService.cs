using DeploymentManager.Tui.Application.Features.AuditLogs.Models;

namespace DeploymentManager.Tui.Application.Features.AuditLogs;

/// <summary>
/// Interface for audit log service.
/// </summary>
public interface IAuditLogsService
{
    /// <summary>
    /// Retrieves customer audit logs.
    /// </summary>
    /// <returns>List of customer audit logs if retrieval succeeds, otherwise <c>null</c>.</returns>
    Task<List<AuditLog>?> GetCustomerAuditLogsAsync(Guid customerId, CancellationToken ct);

    /// <summary>
    /// Retrieves agent audit logs.
    /// </summary>
    /// <returns>List of agent audit logs if retrieval succeeds, otherwise <c>null</c>.</returns>
    Task<List<AuditLog>?> GetAgentAuditLogsAsync(Guid agentId, CancellationToken ct);
}