using DeploymentManager.Tui.Application.Features.AuditLogs.Enums;

namespace DeploymentManager.Tui.Presentation.ViewModels;

/// <summary>
/// View model representing an audit log.
/// </summary>
public sealed class AuditLogViewModel
{
    public DateTimeOffset CreatedAt { get; set; }
    public AuditLogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
}