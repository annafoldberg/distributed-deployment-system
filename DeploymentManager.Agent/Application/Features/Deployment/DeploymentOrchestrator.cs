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
        var result = await _apiClient.GetInstallationPackageAsync(ct);
        
        if (result.Status == PackageRetrievalStatus.Failed)
        {
            _logger.LogWarning("Failed to retrieve installation package.");
            return DeploymentResult.RetrievalFailed;
        }

        if (result.Status == PackageRetrievalStatus.NoUpdateRequired)
        {
            _logger.LogInformation("No update required.");
            return DeploymentResult.NoUpdateRequired;
        }

        if (result.Status == PackageRetrievalStatus.UpdateAvailable)
        {
            if (result.InstallationPackage is null)
            {
                _logger.LogError("Update available, but installation package was null.");
                return DeploymentResult.RetrievalFailed;
            }

            var installationResult = await _packageInstaller.InstallPackageAsync(result.InstallationPackage, ct);
            if (installationResult == InstallationResult.Failed)
            {
                var errorMessage = "Failed to install package.";
                _logger.LogWarning(errorMessage);
                await _apiClient.ReportInstallationResultAsync(false, null, errorMessage, ct);
                return DeploymentResult.InstallationFailed;
            }

            await _apiClient.ReportInstallationResultAsync(true, result.InstallationPackage.Version, null, ct);
            return DeploymentResult.Succeeded;
        }

        _logger.LogWarning("Unknown package retrieval status: {Status}.", result.Status);
        return DeploymentResult.RetrievalFailed;
    }
}