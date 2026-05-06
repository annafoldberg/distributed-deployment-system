using MediatR;
using Microsoft.AspNetCore.Mvc;
using DeploymentManager.Api.Application.Features.Customers.Queries;
using Microsoft.AspNetCore.Authorization;
using DeploymentManager.Api.Presentation.Extensions;

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
    [Authorize(Policy = "Cli")]
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
    /// <param name="customerId">Customer identifier.</param>
    /// <returns>The customer if retrieval succeeds, otherwise HTTP error response.</returns>
    [Authorize(Policy = "Cli")]
    [HttpGet("{customerId}")]
    public async Task<IActionResult> GetCustomerById([FromRoute] Guid customerId, CancellationToken ct)
    {
        var query = new GetCustomerByIdQuery(customerId);
        var result = await _mediator.Send(query, ct);
        
        return result.ToActionResult();
    }
}

// Sources:
// MediatR: https://github.com/LuckyPennySoftware/MediatR/wiki