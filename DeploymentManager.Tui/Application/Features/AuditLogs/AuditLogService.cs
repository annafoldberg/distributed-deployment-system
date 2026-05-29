using DeploymentManager.Tui.Application.Features.AuditLogs;
using DeploymentManager.Tui.Application.Features.AuditLogs.Models;
using DeploymentManager.Tui.Application.Features.Customers.Interfaces;
using DeploymentManager.Tui.Application.Features.Customers.Models;
using Microsoft.Extensions.Logging;

namespace DeploymentManager.Tui.Application.Features.Customers;

/// <summary>
/// Service for retrieving customers.
/// </summary>
public sealed class AuditLogsService : IAuditLogsService
{
    private readonly IDeploymentManagerApiClient _apiClient;
    private readonly ILogger<AuditLogsService> _logger;

    public AuditLogsService(IDeploymentManagerApiClient apiClient, ILogger<AuditLogsService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<List<AuditLog>?> GetCustomerAuditLogsAsync(Guid customerId, CancellationToken ct)
    {
        var auditLogs = await _apiClient.GetCustomerAuditLogsAsync(customerId, ct);
        return auditLogs;
    }

    public async Task<List<AuditLog>?> GetAgentAuditLogsAsync(Guid agentId, CancellationToken ct)
    {
        var auditLogs = await _apiClient.GetAgentAuditLogsAsync(agentId, ct);
        return auditLogs;
    }
}