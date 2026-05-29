
using DeploymentManager.Api.Domain.Enums;

namespace DeploymentManager.Api.Application.Common.Interfaces;

/// <summary>
/// Inserts audit log entries into the database.
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Creates an audit log linked to a customer.
    /// </summary>
    /// <param name="customerId">Identifier of the customer to link the audit log to.</param>
    /// <param name="level">Severity level of the audit log entry.</param>
    /// <param name="message">Description of the audited event.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddCustomerLogAsync(int customerId, AuditLogLevel level, string message, CancellationToken ct);

    /// <summary>
    /// Creates an audit log linked to an agent.
    /// </summary>
    /// <param name="agentId">Identifier of the agent to link the audit log to.</param>
    /// <param name="level">Severity level of the audit log entry.</param>
    /// <param name="message">Description of the audited event.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAgentLogAsync(int agentId, AuditLogLevel level, string message, CancellationToken ct);
}