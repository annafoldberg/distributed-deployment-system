using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using DeploymentManager.Api.Application.Common.Interfaces;
using DeploymentManager.Api.Application.Features.Deployments.Errors;
using DeploymentManager.Api.Application.Features.Common.Errors;
using DeploymentManager.Api.Domain.Entities;

namespace DeploymentManager.Api.Application.Features.Deployments.Commands;

/// <summary>
/// Handles reporting the result of an installation attempt for an agent.
/// </summary>
public sealed class ReportInstallationResultCommandHandler : IRequestHandler<ReportInstallationResultCommand, Result>
{
    private readonly IDeploymentManagerDbContext _context;
    private readonly ILogger<ReportInstallationResultCommandHandler> _logger;

    public ReportInstallationResultCommandHandler(IDeploymentManagerDbContext context, ILogger<ReportInstallationResultCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(ReportInstallationResultCommand request, CancellationToken ct)
    {
        var agent = await _context.Agents
            .FirstOrDefaultAsync(a => a.PublicId == request.AgentId, ct);

        if (agent is null)
        {
            _logger.LogWarning("Agent {AgentId} not found in database.", request.AgentId);
            return Result.Fail(new AgentNotFoundError());
        }

        if (!request.Succeeded)
        {
            _logger.LogWarning("Installation attempt failed for agent {AgentId} with error: {ErrorMessage}", request.AgentId, request.ErrorMessage);
            return Result.Ok();
        }

        var installedRelease = await _context.Releases
            .FirstOrDefaultAsync(r => r.Version == request.InstalledVersion, ct);
            
        if (installedRelease is null)
        {
            _logger.LogWarning("Release with version {InstalledVersion} not found in database.", request.InstalledVersion);
            return Result.Fail(new ReleaseNotFoundError());
        }

        var installation = await _context.Installations
            .FirstOrDefaultAsync(i => i.AgentId == agent.Id, ct);

        if (installation is not null && installation.ReleaseId == installedRelease.Id)
        {
            _logger.LogInformation("Version {InstalledVersion} already installed for agent {AgentId}", request.InstalledVersion, request.AgentId);
            return Result.Ok();
        }

        if (installation is null)
        {
            installation = new Installation
            {
                AgentId = agent.Id,
                ReleaseId = installedRelease.Id
            };
            
            _context.Installations.Add(installation);
        }
        else installation.ReleaseId = installedRelease.Id;
        
        await _context.SaveChangesAsync(ct);

        return Result.Ok();
    }
}