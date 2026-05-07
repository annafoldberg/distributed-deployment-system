using MediatR;
using Microsoft.AspNetCore.Mvc;
using DeploymentManager.Api.Application.Features.Customers.Queries;
using Microsoft.AspNetCore.Authorization;
using DeploymentManager.Api.Presentation.Extensions;
using DeploymentManager.Api.Application.Features.Customers.Commands;

namespace DeploymentManager.Api.Presentation.Controllers;

/// <summary>
/// Exposes customer endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves customers.
    /// </summary>
    /// <returns>All customers if retrieval succeeds, otherwise HTTP error response.</returns>
    [Authorize(Policy = "Tui")]
    [HttpGet]
    public async Task<IActionResult> GetCustomers(CancellationToken ct)
    {
        var query = new GetCustomersQuery();
        var result = await _mediator.Send(query, ct);
        
        return result.ToActionResult();
    }

    /// <summary>
    /// Retrieves a single customer.
    /// </summary>
    /// <param name="customerId">Identifier of the customer to retrieve.</param>
    /// <returns>The customer if retrieval succeeds, otherwise HTTP error response.</returns>
    [Authorize(Policy = "Tui")]
    [HttpGet("{customerId}")]
    public async Task<IActionResult> GetCustomerById([FromRoute] Guid customerId, CancellationToken ct)
    {
        var query = new GetCustomerByIdQuery(customerId);
        var result = await _mediator.Send(query, ct);
        
        return result.ToActionResult();
    }

    /// <summary>
    /// Updates the desired version for a customer.
    /// </summary>
    /// <param name="customerId">Identifier of the customer to update.</param>
    /// <param name="desiredVersion">Desired version to set for the customer.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success if the desired version is updated, otherwise HTTP error response.</returns>
    [Authorize(Policy = "Tui")]
    [HttpPatch("{customerId}/desired-version")]
    public async Task<IActionResult> UpdateDesiredVersion(
        [FromRoute] Guid customerId,
        [FromBody] string desiredVersion,
        CancellationToken ct)
    {
        var command = new UpdateDesiredVersionCommand(customerId, desiredVersion);
        var result = await _mediator.Send(command, ct);
        
        return result.ToActionResult();
    }
}

// Sources:
// MediatR: https://github.com/LuckyPennySoftware/MediatR/wiki