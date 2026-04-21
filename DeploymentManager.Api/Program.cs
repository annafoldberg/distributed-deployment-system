using DeploymentManager.Api.Application.Features.InstallationPackages.Queries;
using FluentValidation;
using DeploymentManager.Api.Infrastructure.ExternalServices.GitHub;
using DeploymentManager.Api.Application.Features.InstallationPackages.Interfaces;
using System.Net.Http.Headers;
using Microsoft.Net.Http.Headers;
using DeploymentManager.Api.Application.Common.Behaviors;
using DeploymentManager.Api.Presentation.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using DeploymentManager.Api.Infrastructure.Configuration;
using DeploymentManager.Api.Infrastructure.Persistence.Context;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using DeploymentManager.Api.Infrastructure.Persistence.Seeding;
using DeploymentManager.Api.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using DeploymentManager.Api.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using DeploymentManager.Api.Presentation.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Configure database
builder.Services
    .AddOptions<DatabaseOptions>()
    .Bind(builder.Configuration.GetSection(DatabaseOptions.SectionName))
    .Validate(options =>
        !string.IsNullOrWhiteSpace(options.Host) &&
        !string.IsNullOrWhiteSpace(options.Name) &&
        !string.IsNullOrWhiteSpace(options.User) &&
        !string.IsNullOrWhiteSpace(options.Password),
        "Database configuration is incomplete.")
    .ValidateOnStart();

// Configure agent seeding
builder.Services
    .AddOptions<AgentSeedOptions>()
    .Bind(builder.Configuration.GetSection(AgentSeedOptions.SectionName))
    .Validate(options =>
        options.Agents.Count > 0 &&
        options.Agents.All(a =>
            a.PublicId != Guid.Empty &&
            !string.IsNullOrWhiteSpace(a.ApiKey) &&
            !string.IsNullOrWhiteSpace(a.Platform)),
        "Agent seeding configuration is incomplete.")
    .ValidateOnStart();

// Register password hasher for agent API keys
builder.Services.AddScoped<IPasswordHasher<Agent>, PasswordHasher<Agent>>();

// Register and seed DbContext
builder.Services.AddDbContext<DeploymentManagerDbContext>((serviceProvider, options) =>
{
    var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    var agentSeedOptions = serviceProvider.GetRequiredService<IOptions<AgentSeedOptions>>().Value;
    var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher<Agent>>();

    var connectionString =
        $"Server={databaseOptions.Host};" +
        $"Database={databaseOptions.Name};" +
        $"User Id={databaseOptions.User};" +
        $"Password={databaseOptions.Password};" +
        "TrustServerCertificate=True;" +
        "Encrypt=True;";
    
    options.UseSqlServer(connectionString)
        .UseSeeding((context, _) =>
        {
            DeploymentManagerDbContextSeeder.SeedAsync(context, agentSeedOptions, passwordHasher, CancellationToken.None)
                .GetAwaiter().GetResult();
        })
        .UseAsyncSeeding(async (context, _, ct) =>
        {
            await DeploymentManagerDbContextSeeder.SeedAsync(context, agentSeedOptions, passwordHasher, ct);
        });
});
builder.Services.AddScoped<IDeploymentManagerDbContext>(sp => sp.GetRequiredService<DeploymentManagerDbContext>());

// Configure GitHub
builder.Services
    .AddOptions<GitHubOptions>()
    .Bind(builder.Configuration.GetSection(GitHubOptions.SectionName))
    .Validate(options =>
        !string.IsNullOrWhiteSpace(options.Token) &&
        !string.IsNullOrWhiteSpace(options.Owner) &&
        !string.IsNullOrWhiteSpace(options.Repository),
        "GitHub configuration is incomplete.")
    .ValidateOnStart();

// Sources:
// HTTP requests: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-10.0
// GitHub Releases API: https://docs.github.com/en/rest/releases/releases?apiVersion=2026-03-10
builder.Services.AddHttpClient<IPackageProvider, GitHubPackageProvider>((serviceProvider, httpClient) =>
{
    var gitHubOptions = serviceProvider.GetRequiredService<IOptions<GitHubOptions>>().Value;

    httpClient.BaseAddress = new Uri("https://api.github.com/");
    httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", gitHubOptions.Token);
    httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2026-03-10");
    httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "DeploymentManager.Api");
    httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/vnd.github+json");
});

builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

// Register authentication scheme
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = AgentApiKeyAuthenticationHandler.SchemeName;
        options.DefaultChallengeScheme = AgentApiKeyAuthenticationHandler.SchemeName;
    })
    .AddScheme<AuthenticationSchemeOptions, AgentApiKeyAuthenticationHandler>(
        AgentApiKeyAuthenticationHandler.SchemeName,
        _ => {});

builder.Services.AddAuthorization();

// Source: https://github.com/LuckyPennySoftware/MediatR
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssemblyContaining<Program>();
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

// Source: https://docs.fluentvalidation.net/en/latest/aspnet.html
builder.Services.AddValidatorsFromAssemblyContaining<GetInstallationPackageQueryValidator>();

// Source: https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ValidationExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live")
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready")
});

app.Run();