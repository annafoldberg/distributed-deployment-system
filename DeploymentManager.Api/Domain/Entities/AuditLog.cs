using DeploymentManager.Api.Domain.Enums;

namespace DeploymentManager.Api.Domain.Entities;

/// <summary>
/// Represents an audit log entry stored in the database.
/// </summary>
public class AuditLog
{
    public int Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public AuditLogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? AgentId { get; set; }
    public Agent? Agent { get; set; }
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
}