using DeploymentManager.Cli.Application.Features.Customers.Models;

namespace DeploymentManager.Cli.Application.Features.Customers;

/// <summary>
/// Interface for customer service.
/// </summary>
public interface ICustomersService
{
    /// <summary>
    /// Retrieves customers.
    /// </summary>
    /// <returns>List of customers if retrieval succeeds, otherwise <c>null</c>.</returns>
    Task<List<Customer>?> GetCustomersAsync(CancellationToken ct);

    /// <summary>
    /// Retrieves a single customer.
    /// </summary>
    /// <param name="customerId">The ID of the customer to retrieve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The customer if retrieval succeeds, otherwise <c>null</c>.</returns>
    Task<Customer?> GetCustomerByIdAsync(Guid customerId, CancellationToken ct);
}