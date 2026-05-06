using System.Net.Http.Json;
using DeploymentManager.Cli.Application.Features.Customers.Interfaces;
using DeploymentManager.Cli.Application.Features.Customers.Models;
using Microsoft.Extensions.Logging;

namespace DeploymentManager.Cli.Infrastructure.Api;

/// <summary>
/// Performs requests to Deployment Manager API.
/// </summary>
public sealed class DeploymentManagerApiClient : IDeploymentManagerApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DeploymentManagerApiClient> _logger;

    public DeploymentManagerApiClient(HttpClient httpClient, ILogger<DeploymentManagerApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves customers from Deployment Manager API.
    /// </summary>
    /// <returns>List of customers if retrieval succeeds, otherwise <c>null</c>.</returns>
    public async Task<List<Customer>?> GetCustomersAsync(CancellationToken ct)
    {
        var requestUri = "customers/";
        using var response = await _httpClient.GetAsync(requestUri, ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to retrieve customers. Status code: {StatusCode}.", response.StatusCode);
            return null;
        }

        var customers = await response.Content.ReadFromJsonAsync<List<Customer>>(ct);
        if (customers is null)
        {
            _logger.LogWarning("Customers response could not be deserialized.");
            return null;
        }

        return customers;
    }

    /// <summary>
    /// Retrieve a single customer from Deployment Manager API.
    /// </summary>
    /// <returns>The customer if retrieval succeeds, otherwise <c>null</c>.</returns>
    public async Task<Customer?> GetCustomerByIdAsync(Guid customerId, CancellationToken ct)
    {
        var requestUri = $"customers/{customerId}/";
        using var response = await _httpClient.GetAsync(requestUri, ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to retrieve customer {CustomerId}. Status code: {StatusCode}.", customerId, response.StatusCode);
            return null;
        }

        var customer = await response.Content.ReadFromJsonAsync<Customer>(ct);
        if (customer is null)
        {
            _logger.LogWarning("Customer response could not be deserialized.");
            return null;
        }

        return customer;
    }
}

// Source: https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-10.0