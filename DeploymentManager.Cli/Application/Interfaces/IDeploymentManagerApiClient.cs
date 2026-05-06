using DeploymentManager.Cli.Application.Features.Customers.Models;

namespace DeploymentManager.Cli.Application.Features.Customers.Interfaces;

/// <summary>
/// Interface for Deployment Manager API requests.
/// </summary>
public interface IDeploymentManagerApiClient
{
    /// <summary>
    /// Retrieves customers from Deployment Manager API.
    /// </summary>
    /// <returns>List of customers if retrieval succeeds, otherwise <c>null</c>.</returns>
    Task<List<Customer>?> GetCustomersAsync(CancellationToken ct);

    /// <summary>
    /// Retrieves a single customer from Deployment Manager API.
    /// </summary>
    /// <returns>The customer if retrieval succeeds, otherwise <c>null</c>.</returns>
    Task<Customer?> GetCustomerByIdAsync(Guid customerId, CancellationToken ct);
}