using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DeploymentManager.Api.Presentation.Extensions;
using DeploymentManager.Api.Application.Features.AuditLogs.Queries;

namespace DeploymentManager.Api.Presentation.Controllers;

/// <summary>
/// Exposes audit log endpoints.
/// </summary>
[ApiController]
[Route("api/audit-logs")]
public class AuditLogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves customer audit logs.
    /// </summary>
    /// <returns>Customer audit logs if retrieval succeeds, otherwise HTTP error response.</returns>
    [Authorize(Policy = "Tui")]
    [HttpGet("customers/{customerId}")]
    public async Task<IActionResult> GetCustomerAuditLogs([FromRoute] Guid customerId, CancellationToken ct)
    {
        var query = new GetCustomerAuditLogsQuery(customerId);
        var result = await _mediator.Send(query, ct);
        
        return result.ToActionResult();
    }

    /// <summary>
    /// Retrieves agent audit logs.
    /// </summary>
    /// <returns>Customer audit logs if retrieval succeeds, otherwise HTTP error response.</returns>
    [Authorize(Policy = "Tui")]
    [HttpGet("agents/{agentId}")]
    public async Task<IActionResult> GetAgentAuditLogs([FromRoute] Guid agentId, CancellationToken ct)
    {
        var query = new GetAgentAuditLogsQuery(agentId);
        var result = await _mediator.Send(query, ct);
        
        return result.ToActionResult();
    }
}

// Sources:
// MediatR: https://github.com/LuckyPennySoftware/MediatR/wiki