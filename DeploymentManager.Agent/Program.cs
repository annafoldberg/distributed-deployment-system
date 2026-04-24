using DeploymentManager.Agent;
using DeploymentManager.Agent.Application.Features.Deployment;
using DeploymentManager.Agent.Application.Features.Deployment.Interfaces;
using DeploymentManager.Agent.Infrastructure.Api;
using DeploymentManager.Agent.Infrastructure.Configuration;
using DeploymentManager.Agent.Workers;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

var builder = Host.CreateApplicationBuilder(args);

// Configure agent identity
builder.Services
    .AddOptions<AgentIdentityOptions>()
    .Bind(builder.Configuration.GetSection(AgentIdentityOptions.SectionName))
    .Validate(options =>
        options.AgentId != Guid.Empty &&
        !string.IsNullOrWhiteSpace(options.ApiKey),
        "Agent identity configuration is incomplete.")
    .ValidateOnStart();

// Configure API
builder.Services
    .AddOptions<ApiOptions>()
    .Bind(builder.Configuration.GetSection(ApiOptions.SectionName))
    .Validate(options =>
        !string.IsNullOrWhiteSpace(options.BaseUrl),
        "API configuration is incomplete.")
    .ValidateOnStart();

// Register HTTP client
builder.Services.AddHttpClient<IDeploymentManagerApiClient, DeploymentManagerApiClient>((serviceProvider, httpClient) =>
{
    var agentIdentityOptions = serviceProvider.GetRequiredService<IOptions<AgentIdentityOptions>>().Value;
    var apiOptions = serviceProvider.GetRequiredService<IOptions<ApiOptions>>().Value;

    httpClient.BaseAddress = new Uri(apiOptions.BaseUrl);

    httpClient.DefaultRequestHeaders.Add("X-API-KEY", agentIdentityOptions.ApiKey);
    httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "DeploymentManager.Agent");
});

builder.Services.AddScoped<DeploymentOrchestrator>();
builder.Services.AddHostedService<DeploymentWorker>();

var host = builder.Build();
host.Run();