using DeploymentManager.Tui.Application.Features.AuditLogs.Enums;
using DeploymentManager.Tui.Application.Features.AuditLogs.Models;
using DeploymentManager.Tui.Presentation.ViewModels;

namespace DeploymentManager.Tui.Presentation.Mapping;

/// <summary>
/// Maps audit log models to audit log view models.
/// </summary>
public static class AuditLogViewModelMapper
{
    /// <summary>
    /// Creates an audit log view model from audit log data.
    /// </summary>
    /// <param name="auditLog">Audit log to map to view model.</param>
    /// <returns>Mapped audit log view model.</returns>
    public static AuditLogViewModel ToViewModel(AuditLog auditLog)
    {
        return new AuditLogViewModel
        {
            CreatedAt = auditLog.CreatedAt,
            Level = auditLog.Level,
            Message = auditLog.Message
        };
    }

}