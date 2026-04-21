using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using DeploymentManager.Api.Application.Common.Interfaces;
using DeploymentManager.Api.Application.Features.InstallationPackages.Interfaces;
using DeploymentManager.Api.Application.Features.InstallationPackages.Dtos;
using DeploymentManager.Api.Application.Features.InstallationPackages.Errors;

namespace DeploymentManager.Api.Application.Features.InstallationPackages.Queries;

/// <summary>
/// Handles retrieval of installation packages for an agent.
/// </summary>
public sealed class GetInstallationPackageQueryHandler : IRequestHandler<GetInstallationPackageQuery, Result<InstallationPackageDto>>
{
    private readonly IPackageProvider _provider;
    private readonly IDeploymentManagerDbContext _context;
    private readonly ILogger<GetInstallationPackageQueryHandler> _logger;

    public GetInstallationPackageQueryHandler(IPackageProvider provider, IDeploymentManagerDbContext context, ILogger<GetInstallationPackageQueryHandler> logger)
    {
        _provider = provider;
        _context = context;
        _logger = logger;
    }

    public async Task<Result<InstallationPackageDto>> Handle(GetInstallationPackageQuery request, CancellationToken ct)
    {
        var agent = await _context.Agents
            .Include(a => a.Customer)
            .ThenInclude(c => c.DesiredRelease)
            .FirstOrDefaultAsync(a => a.PublicId == request.AgentId, ct);

        if (agent is null)
        {
            _logger.LogWarning("Agent {AgentId} not found in database.", request.AgentId);
            return Result.Fail(new AgentNotFoundError());
        }

        var platform = agent.Platform;
        var customer = agent.Customer;

        var desiredRelease = customer.DesiredRelease;
        if (desiredRelease is null)
        {
            _logger.LogWarning("Customer {CustomerId} has no desired release.", customer.Id);
            return Result.Fail(new DesiredReleaseNotSetError());
        }

        var installation = await _context.Installations
            .Include(i => i.Release)
            .FirstOrDefaultAsync(i => i.AgentId == agent.Id, ct);

        var currentRelease = installation?.Release;
        if (currentRelease is not null && currentRelease.Version == desiredRelease.Version)
        {
            _logger.LogInformation("Version {Version} is already installed for agent {AgentId}.", desiredRelease.Version, agent.PublicId);
            return Result.Fail(new NoUpdateRequiredError());
        }

        var package = await _provider.FetchPackageAsync(platform, desiredRelease.Version, ct);
        if (package is null)
        {
            _logger.LogWarning("Package for platform {Platform} with version {Version} not found.", platform, desiredRelease.Version);
            return Result.Fail(new PackageNotFoundError());
        }

        return Result.Ok(package);
    }
}