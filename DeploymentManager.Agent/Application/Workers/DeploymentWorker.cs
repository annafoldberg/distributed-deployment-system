using DeploymentManager.Agent.Application.Features.Deployment;

namespace DeploymentManager.Agent.Workers;

/// <summary>
/// Background worker that triggers deployment of the managed software.
/// </summary>
public sealed class DeploymentWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DeploymentWorker> _logger;

    public DeploymentWorker(IServiceScopeFactory scopeFactory, ILogger<DeploymentWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Temporary flow to test feature flow
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Deployment worker running at: {Time}", DateTimeOffset.Now);
            
            using var scope = _scopeFactory.CreateScope();
            var deploymentOrchestrator = scope.ServiceProvider.GetRequiredService<DeploymentOrchestrator>();

            var result = await deploymentOrchestrator.ExecuteAsync(stoppingToken);

            _logger.LogInformation("Deployment finished with result: {Result}.", result);

            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Deployment worker is stopping.");
        await base.StopAsync(stoppingToken);
    }
}

// Source: https://learn.microsoft.com/en-us/dotnet/core/extensions/scoped-service