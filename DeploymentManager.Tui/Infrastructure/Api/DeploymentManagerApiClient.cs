using System.Net.Http.Json;
using DeploymentManager.Tui.Application.Features.Customers.Interfaces;
using DeploymentManager.Tui.Application.Features.Customers.Models;
using Microsoft.Extensions.Logging;

namespace DeploymentManager.Tui.Infrastructure.Api;

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

    public async Task<OperationResult> UpdateDesiredVersionAsync(Guid customerId, string desiredVersion, CancellationToken ct)
    {
        var requestUri = $"customers/{customerId}/desired-version";
        using var response = await _httpClient.PatchAsJsonAsync(requestUri, desiredVersion, ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to update desired version to {DesiredVersion} for customer {CustomerId}. Status code: {StatusCode}.",
                desiredVersion, customerId, response.StatusCode);

            var errorMessages = await response.Content.ReadFromJsonAsync<List<string>>(ct);
            
            var errorMessage = errorMessages?.FirstOrDefault() ?? "Failed to update desired version.";

            return OperationResult.Failure(errorMessage);
        }

        return OperationResult.Success();
    }
}

// Source: https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-10.0