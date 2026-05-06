using DeploymentManager.Cli.Application.Features.Customers.Interfaces;
using DeploymentManager.Cli.Application.Features.Customers.Models;
using Microsoft.Extensions.Logging;

namespace DeploymentManager.Cli.Application.Features.Customers;

/// <summary>
/// Service for retrieving customers.
/// </summary>
public sealed class CustomersService : ICustomersService
{
    private readonly IDeploymentManagerApiClient _apiClient;
    private readonly ILogger<CustomersService> _logger;

    public CustomersService(IDeploymentManagerApiClient apiClient, ILogger<CustomersService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<List<Customer>?> GetCustomersAsync(CancellationToken ct)
    {
        var customers = await _apiClient.GetCustomersAsync(ct);
        if (customers is not null && customers.Count == 0)
            _logger.LogWarning("No customers found.");

        return customers;
    }

    public async Task<Customer?> GetCustomerByIdAsync(Guid customerId, CancellationToken ct)
    {
        var customer = await _apiClient.GetCustomerByIdAsync(customerId, ct);
        if (customer is null)
            _logger.LogWarning("Customer {CustomerId} not found.", customerId);

        return customer;
    }
}