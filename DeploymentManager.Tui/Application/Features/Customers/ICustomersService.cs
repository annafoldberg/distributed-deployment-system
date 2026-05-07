using DeploymentManager.Tui.Application.Features.Customers.Models;

namespace DeploymentManager.Tui.Application.Features.Customers;

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
    /// <param name="customerId">Identifier of the customer to retrieve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The customer if retrieval succeeds, otherwise <c>null</c>.</returns>
    Task<Customer?> GetCustomerByIdAsync(Guid customerId, CancellationToken ct);

    /// <summary>
    /// Updates the desired version for a customer.
    /// </summary>
    /// <param name="customerId">Identifier of the customer to update.</param>
    /// <param name="desiredVersion">Desired version to set for the customer.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Operation result representing whether the desired version was updated successfully.
    /// </returns>
    Task<OperationResult> UpdateDesiredVersionAsync(Guid customerId, string desiredVersion, CancellationToken ct);
}