using DeploymentManager.Agent.Application.Features.Deployment.Interfaces;
using DeploymentManager.Agent.Application.Features.Deployment.Results;

namespace DeploymentManager.Agent.Application.Features.Deployment;

/// <summary>
/// Orchestrates deployments of the managed software.
/// </summary>
public sealed class DeploymentOrchestrator
{
    private readonly IDeploymentManagerApiClient _apiClient;
    private readonly IPackageInstaller _packageInstaller;
    private readonly ILogger<DeploymentOrchestrator> _logger;
    public DeploymentOrchestrator(IDeploymentManagerApiClient apiClient, IPackageInstaller packageInstaller, ILogger<DeploymentOrchestrator> logger)
    {
        _apiClient = apiClient;
        _packageInstaller = packageInstaller;
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

        var installationResult = await _packageInstaller.InstallPackageAsync(installationPackage, ct);
        if (installationResult == InstallationResult.Failed)
        {
            _logger.LogWarning("Failed to install package.");
            return DeploymentResult.InstallationFailed;
        }

        return DeploymentResult.Succeeded;
    }
}