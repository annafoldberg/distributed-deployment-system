using DeploymentManager.Cli.Application.Features.Customers;
using DeploymentManager.Cli.Application.Features.Customers.Interfaces;
using DeploymentManager.Cli.Infrastructure.Api;
using DeploymentManager.Cli.Infrastructure.Configuration;
using DeploymentManager.Cli.Presentation.Menus.Agents;
using DeploymentManager.Cli.Presentation.Menus.Customers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

var builder = Host.CreateApplicationBuilder(args);

// Configure CLI identity
builder.Services
    .AddOptions<CliIdentityOptions>()
    .Bind(builder.Configuration.GetSection(CliIdentityOptions.SectionName))
    .Validate(options =>
        !string.IsNullOrWhiteSpace(options.ApiKey),
        "CLI identity configuration is incomplete.")
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
    var cliIdentityOptions = serviceProvider.GetRequiredService<IOptions<CliIdentityOptions>>().Value;
    var apiOptions = serviceProvider.GetRequiredService<IOptions<ApiOptions>>().Value;

    httpClient.BaseAddress = new Uri(apiOptions.BaseUrl);

    httpClient.DefaultRequestHeaders.Add("X-API-KEY", cliIdentityOptions.ApiKey);
    httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "DeploymentManager.Cli");
});

builder.Services.AddTransient<ICustomersService, CustomersService>();
builder.Services.AddTransient<CustomersMenu>();
builder.Services.AddTransient<AgentsMenu>();

var host = builder.Build();

var customersMenu = host.Services.GetRequiredService<CustomersMenu>();
await customersMenu.ShowAsync(ct: default);