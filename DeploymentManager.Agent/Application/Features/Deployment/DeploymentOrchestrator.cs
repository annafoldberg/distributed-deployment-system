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
            var errorMessage = "Failed to retrieve installation package.";
            _logger.LogWarning(errorMessage);
            await _apiClient.ReportInstallationResultAsync(false, null, errorMessage, ct);
            return DeploymentResult.RetrievalFailed;
        }

        var installationResult = await _packageInstaller.InstallPackageAsync(installationPackage, ct);
        if (installationResult == InstallationResult.Failed)
        {
            var errorMessage = "Failed to install package.";
            _logger.LogWarning(errorMessage);
            await _apiClient.ReportInstallationResultAsync(false, null, errorMessage, ct);
            return DeploymentResult.InstallationFailed;
        }
        await _apiClient.ReportInstallationResultAsync(true, installationPackage.Version, null, ct);
        return DeploymentResult.Succeeded;
    }
}