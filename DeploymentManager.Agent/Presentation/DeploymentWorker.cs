using DeploymentManager.Agent.Application.Features.Deployment;
using DeploymentManager.Agent.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace DeploymentManager.Agent.Presentation;

/// <summary>
/// Background worker that triggers deployment of the managed software.
/// </summary>
public sealed class DeploymentWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DeploymentWorkerOptions _options;
    private readonly ILogger<DeploymentWorker> _logger;

    public DeploymentWorker(IServiceScopeFactory scopeFactory, IOptions<DeploymentWorkerOptions> options, ILogger<DeploymentWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Deployment worker running at: {Time}", DateTimeOffset.Now);
            
                using var scope = _scopeFactory.CreateScope();
                var deploymentOrchestrator = scope.ServiceProvider.GetRequiredService<DeploymentOrchestrator>();

                var result = await deploymentOrchestrator.ExecuteAsync(stoppingToken);

                _logger.LogInformation("Deployment finished with result: {Result}.", result);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal shutdown
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "API is temporarily unavailable. The agent will retry on the next polling attempt.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during polling. The agent will retry on the next polling attempt.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.IntervalSeconds), stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Deployment worker is stopping.");
        await base.StopAsync(stoppingToken);
    }
}

// Source: https://learn.microsoft.com/en-us/dotnet/core/extensions/scoped-service