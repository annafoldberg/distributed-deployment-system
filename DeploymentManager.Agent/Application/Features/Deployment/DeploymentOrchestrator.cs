using DeploymentManager.Agent.Application.Features.Deployment.Interfaces;

namespace DeploymentManager.Agent.Application.Features.Deployment;

/// <summary>
/// Orchestrates deployments of the managed software.
/// </summary>
public sealed class DeploymentOrchestrator
{
    private readonly IDeploymentManagerApiClient _apiClient;
    private readonly ILogger<DeploymentOrchestrator> _logger;
    public DeploymentOrchestrator(IDeploymentManagerApiClient apiClient, ILogger<DeploymentOrchestrator> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }
    
    /// <summary>
    /// Executes the deployment process.
    /// </summary>
    public async Task<DeploymentResult> ExecuteAsync(CancellationToken ct)
    {
        var installationPackage = await _apiClient.GetInstallationPackageAsync(ct);
        if (installationPackage is null)
        {
            _logger.LogWarning("Failed to retrieve installation package.");
            return DeploymentResult.RetrievalFailed;
        }

        return DeploymentResult.Succeeded;
    }
}