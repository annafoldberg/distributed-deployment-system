using DeploymentManager.Api.Application.Features.InstallationPackages.Queries;
using MediatR;
using FluentValidation;
using DeploymentManager.Api.Infrastructure.ExternalServices.GitHub;
using DeploymentManager.Api.Application.Features.InstallationPackages.Interfaces;
using System.Net.Http.Headers;
using Microsoft.Net.Http.Headers;
using DeploymentManager.Api.Application.Common.Behaviors;
using DeploymentManager.Api.Presentation.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddControllers();
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

// Sources:
// HTTP requests: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-10.0
// GitHub Releases API: https://docs.github.com/en/rest/releases/releases?apiVersion=2026-03-10
builder.Services.Configure<GitHubOptions>(builder.Configuration.GetSection("GitHub"));
builder.Services.AddHttpClient<IPackageProvider, GitHubPackageProvider>(httpClient =>
{
    httpClient.BaseAddress = new Uri("https://api.github.com/");
    httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", builder.Configuration["GitHub:Token"]);
    httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2026-03-10");
    httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "DeploymentManager.Api");
    httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/vnd.github+json");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseMiddleware<ValidationExceptionMiddleware>();
app.MapControllers();
app.UseAuthorization();

app.Run();