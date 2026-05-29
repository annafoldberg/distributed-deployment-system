using DeploymentManager.Tui.Application.Features.AuditLogs.Enums;

namespace DeploymentManager.Tui.Application.Features.AuditLogs.Models;

/// <summary>
/// Represents an audit log entry stored in the database.
/// </summary>
public class AuditLog
{
    public DateTimeOffset CreatedAt { get; set; }
    public AuditLogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
}