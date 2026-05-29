using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using DeploymentManager.Api.Application.Common.Interfaces;
using DeploymentManager.Api.Application.Features.Deployments.Interfaces;
using DeploymentManager.Api.Application.Features.Deployments.Models;
using DeploymentManager.Api.Application.Features.Deployments.Errors;
using DeploymentManager.Api.Domain.Enums;
using DeploymentManager.Api.Application.Features.Common.Errors;

namespace DeploymentManager.Api.Application.Features.Deployments.Queries;

/// <summary>
/// Handles retrieval of installation packages for an agent.
/// </summary>
public sealed class GetInstallationPackageQueryHandler : IRequestHandler<GetInstallationPackageQuery, Result<GetInstallationPackageQueryResult>>
{
    private readonly IPackageProvider _provider;
    private readonly IDeploymentManagerDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<GetInstallationPackageQueryHandler> _logger;

    public GetInstallationPackageQueryHandler(IPackageProvider provider, IDeploymentManagerDbContext context, IAuditLogService auditLogService, ILogger<GetInstallationPackageQueryHandler> logger)
    {
        _provider = provider;
        _context = context;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<Result<GetInstallationPackageQueryResult>> Handle(GetInstallationPackageQuery request, CancellationToken ct)
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

            await _auditLogService.AddAgentLogAsync(
                agent.Id,
                AuditLogLevel.Information,
                $"Installation package retrieval skipped: Desired version not set.",
                ct);

            return Result.Ok(new GetInstallationPackageQueryResult
            {
                Status = InstallationPackageStatus.DesiredReleaseNotSet
            });
        }

        var installation = await _context.Installations
            .Include(i => i.Release)
            .FirstOrDefaultAsync(i => i.AgentId == agent.Id, ct);

        var currentRelease = installation?.Release;
        if (currentRelease is not null && currentRelease.Version == desiredRelease.Version)
        {
            _logger.LogInformation("Version {Version} is already installed for agent {AgentId}.", desiredRelease.Version, agent.PublicId);

            await _auditLogService.AddAgentLogAsync(
                agent.Id,
                AuditLogLevel.Information,
                $"Installation package retrieval skipped: Version {desiredRelease.Version} already installed.",
                ct);

            return Result.Ok(new GetInstallationPackageQueryResult
            {
                Status = InstallationPackageStatus.NoUpdateRequired
            });
        }

        var version = desiredRelease.Version;
        var package = await _provider.FetchPackageAsync(platform, version, ct);

        if (package is null)
        {
            _logger.LogWarning("Package for platform {Platform} with version {Version} not found.", platform, version);

            await _auditLogService.AddAgentLogAsync(
                agent.Id,
                AuditLogLevel.Warning,
                $"Installation package retrieval failed: Package for version {version} and platform {platform} not found.",
                ct);

            return Result.Fail(new PackageNotFoundError());
        }

        await _auditLogService.AddAgentLogAsync(
            agent.Id,
            AuditLogLevel.Information,
            $"Successfully retrieved installation package for version {version}.",
            ct);

        return Result.Ok(new GetInstallationPackageQueryResult
        {
            Status = InstallationPackageStatus.Available,
            InstallationPackage = package
        });
    }
}