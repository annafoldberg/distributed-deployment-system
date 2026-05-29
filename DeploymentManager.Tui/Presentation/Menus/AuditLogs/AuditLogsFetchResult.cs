using DeploymentManager.Tui.Presentation.ViewModels;

namespace DeploymentManager.Tui.Presentation.Menus.AuditLogs;

/// <summary>
/// Result of fetching audit log view models for audit logs menu.
/// </summary>
public sealed class AuditLogsFetchResult
{
    public List<AuditLogViewModel>? AuditLogs { get; init; }
    public string? Message { get; init; }
}