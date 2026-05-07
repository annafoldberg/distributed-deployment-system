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
using Microsoft.Extensions.Diagnostics.HealthChecks;

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

// Configure customer seeding
builder.Services
    .AddOptions<CustomerSeedOptions>()
    .Bind(builder.Configuration.GetSection(CustomerSeedOptions.SectionName))
    .Validate(options =>
        options.PublicId != Guid.Empty &&
        !string.IsNullOrWhiteSpace(options.CompanyName),
        "Customer seeding configuration is incomplete.")
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

// Configure TUI identity
builder.Services
    .AddOptions<TuiIdentityOptions>()
    .Bind(builder.Configuration.GetSection(TuiIdentityOptions.SectionName))
    .Validate(options =>
        !string.IsNullOrWhiteSpace(options.ApiKeyHash),
        "TUI identity configuration is incomplete.")
    .ValidateOnStart();

// Register password hasher for TUI API keys
builder.Services.AddScoped<IPasswordHasher<TuiApiKeyAuthenticationHandler>, PasswordHasher<TuiApiKeyAuthenticationHandler>>();

// Register and seed DbContext
builder.Services.AddDbContext<DeploymentManagerDbContext>((serviceProvider, options) =>
{
    var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    var customerSeedOptions = serviceProvider.GetRequiredService<IOptions<CustomerSeedOptions>>().Value;
    var agentSeedOptions = serviceProvider.GetRequiredService<IOptions<AgentSeedOptions>>().Value;
    var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher<Agent>>();

    var connectionString =
        $"Server={databaseOptions.Host};" +
        $"Database={databaseOptions.Name};" +
        $"User Id={databaseOptions.User};" +
        $"Password={databaseOptions.Password};" +
        "TrustServerCertificate=True;" +
        "Encrypt=True;";    

    // Source: https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding
    options.UseSqlServer(connectionString)
        .UseSeeding((context, _) =>
        {
            DeploymentManagerDbContextSeeder.SeedAsync(context, customerSeedOptions, agentSeedOptions, passwordHasher, CancellationToken.None)
                .GetAwaiter().GetResult();
        })
        .UseAsyncSeeding(async (context, _, ct) =>
        {
            await DeploymentManagerDbContextSeeder.SeedAsync(context, customerSeedOptions, agentSeedOptions, passwordHasher, ct);
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
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddDbContextCheck<DeploymentManagerDbContext>(tags: new[] { "ready" });

// Register authentication scheme
builder.Services
    .AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, AgentApiKeyAuthenticationHandler>(
        AgentApiKeyAuthenticationHandler.SchemeName,
        _ => { })
    .AddScheme<AuthenticationSchemeOptions, TuiApiKeyAuthenticationHandler>(
        TuiApiKeyAuthenticationHandler.SchemeName,
        _ => { });

// Register named policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Agent", policy =>
    {
        policy.AuthenticationSchemes.Add(AgentApiKeyAuthenticationHandler.SchemeName);
        policy.RequireAuthenticatedUser();
    });
    options.AddPolicy("Tui", policy =>
    {
        policy.AuthenticationSchemes.Add(TuiApiKeyAuthenticationHandler.SchemeName);
        policy.RequireAuthenticatedUser();
    });
});

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

// Source: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying?tabs=dotnet-core-cli#idempotent-sql-scripts
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DeploymentManagerDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ValidationExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/healthz/live", new HealthCheckOptions
{
    Predicate = h => h.Tags.Contains("live")
}).AllowAnonymous();
app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
{
    Predicate = h => h.Tags.Contains("ready")
}).AllowAnonymous();

app.Run();