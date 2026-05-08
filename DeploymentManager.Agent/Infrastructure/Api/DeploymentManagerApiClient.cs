using System.Net.Http.Json;
using DeploymentManager.Agent.Application.Features.Deployment.Interfaces;
using DeploymentManager.Agent.Application.Features.Deployment.Models;
using DeploymentManager.Agent.Application.Features.Deployment.Results;
using DeploymentManager.Agent.Infrastructure.Api.Contracts;
using DeploymentManager.Agent.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace DeploymentManager.Agent.Infrastructure.Api;

/// <summary>
/// Performs requests to Deployment Manager API.
/// </summary>
public sealed class DeploymentManagerApiClient : IDeploymentManagerApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AgentIdentityOptions _options;
    private readonly ILogger<DeploymentManagerApiClient> _logger;

    public DeploymentManagerApiClient(HttpClient httpClient, IOptions<AgentIdentityOptions> options, ILogger<DeploymentManagerApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves installation package from Deployment Manager API.
    /// </summary>
    /// <returns>Installation package if retrieval succeeds, otherwise <c>null</c>.</returns>
    public async Task<InstallationPackage?> GetInstallationPackageAsync(CancellationToken ct)
    {
        var requestUri = $"deployments/{_options.AgentId}/package";
        using var response = await _httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to retrieve installation package. Status code: {StatusCode}.", response.StatusCode);
            return null;
        }

        var version = response.Headers.TryGetValues("X-Release-Version", out var values)
                      ? values.FirstOrDefault()
                      : null;

        if (string.IsNullOrWhiteSpace(version))
        {
            _logger.LogError("Installation package response did not contain deployment version");
            return null;
        }

        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                       ?? response.Content.Headers.ContentDisposition?.FileName;

        if (string.IsNullOrWhiteSpace(fileName))
        {
            _logger.LogError("Installation package response did not contain a filename.");
            return null;
        }

        fileName = fileName.Trim('"');

        using var responseStream = await response.Content.ReadAsStreamAsync(ct);
        
        var content = new MemoryStream();
        await responseStream.CopyToAsync(content, ct);
        content.Position = 0;

        return new InstallationPackage
        {
            Content = content,
            Version = version,
            FileName = fileName
        };
    }

    public async Task ReportInstallationResultAsync(bool succeeded, string? installedVersion, string? errorMessage, CancellationToken ct)
    {
        var requestUri = $"deployments/{_options.AgentId}/result";
        var installationResult = new InstallationResultDto
        {
            Succeeded = succeeded,
            InstalledVersion = succeeded ? installedVersion : null,
            ErrorMessage = succeeded ? null : errorMessage
        };

        using var response = await _httpClient.PostAsJsonAsync(requestUri, installationResult, ct);

        if (!response.IsSuccessStatusCode)
        {
            var responseErrorMessages = await response.Content.ReadFromJsonAsync<List<string>>(ct);
            
            var responseErrorMessage = responseErrorMessages?.FirstOrDefault() ?? "Failed to report installed version.";

            _logger.LogWarning("Failed to report installation result for agent {AgentId}." +
                               "Status code: {StatusCode}. Error message: {ErrorMessage}",
                               _options.AgentId, response.StatusCode, responseErrorMessage);
        }
    }
}

// Sources:
// HttpClient: https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-10.0
// HttpResponseMessage: https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpresponsemessage?view=net-10.0
// ContentDispositionHeaderValue: https://learn.microsoft.com/en-us/dotnet/api/system.net.http.headers.contentdispositionheadervalue?view=net-10.0
// StreamContent: https://learn.microsoft.com/en-us/dotnet/api/system.net.http.streamcontent?view=net-10.0