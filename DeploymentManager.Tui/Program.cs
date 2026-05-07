using DeploymentManager.Tui.Application.Features.Customers;
using DeploymentManager.Tui.Application.Features.Customers.Interfaces;
using DeploymentManager.Tui.Infrastructure.Api;
using DeploymentManager.Tui.Infrastructure.Configuration;
using DeploymentManager.Tui.Presentation.Menus.Agents;
using DeploymentManager.Tui.Presentation.Menus.Customers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

var builder = Host.CreateApplicationBuilder(args);

// Configure TUI identity
builder.Services
    .AddOptions<TuiIdentityOptions>()
    .Bind(builder.Configuration.GetSection(TuiIdentityOptions.SectionName))
    .Validate(options =>
        !string.IsNullOrWhiteSpace(options.ApiKey),
        "TUI identity configuration is incomplete.")
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
    var tuiIdentityOptions = serviceProvider.GetRequiredService<IOptions<TuiIdentityOptions>>().Value;
    var apiOptions = serviceProvider.GetRequiredService<IOptions<ApiOptions>>().Value;

    httpClient.BaseAddress = new Uri(apiOptions.BaseUrl);

    httpClient.DefaultRequestHeaders.Add("X-API-KEY", tuiIdentityOptions.ApiKey);
    httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "DeploymentManager.Tui");
});

builder.Services.AddTransient<ICustomersService, CustomersService>();
builder.Services.AddTransient<CustomersMenu>();
builder.Services.AddTransient<AgentsMenu>();

builder.Logging.ClearProviders();
builder.Logging.AddDebug();

var host = builder.Build();

var customersMenu = host.Services.GetRequiredService<CustomersMenu>();
await customersMenu.ShowAsync(ct: default);