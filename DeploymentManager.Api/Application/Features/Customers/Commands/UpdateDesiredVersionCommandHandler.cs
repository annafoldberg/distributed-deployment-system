using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using DeploymentManager.Api.Application.Common.Interfaces;
using DeploymentManager.Api.Application.Features.Customers.Errors;
using DeploymentManager.Api.Application.Features.Common.Errors;
using DeploymentManager.Api.Domain.Enums;

namespace DeploymentManager.Api.Application.Features.Customers.Commands;

/// <summary>
/// Handles updating the desired version for a customer.
/// </summary>
public sealed class UpdateDesiredVersionCommandHandler : IRequestHandler<UpdateDesiredVersionCommand, Result>
{
    private readonly IDeploymentManagerDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<UpdateDesiredVersionCommandHandler> _logger;

    public UpdateDesiredVersionCommandHandler(IDeploymentManagerDbContext context, IAuditLogService auditLogService, ILogger<UpdateDesiredVersionCommandHandler> logger)
    {
        _context = context;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateDesiredVersionCommand request, CancellationToken ct)
    {
        var customer = await _context.Customers
            .Where(c => c.PublicId == request.CustomerId)
            .Include(c => c.DesiredRelease)
            .FirstOrDefaultAsync(ct);

        if (customer is null)
        {
            _logger.LogWarning("Customer {CustomerId} not found in database.", request.CustomerId);
            return Result.Fail(new CustomerNotFoundError());
        }

        var previousReleaseVersion = customer.DesiredRelease?.Version ?? "none";

        var desiredRelease = await _context.Releases
            .FirstOrDefaultAsync(r => r.Version == request.DesiredVersion, ct);
            
        if (desiredRelease is null)
        {
            _logger.LogWarning("Release with version {DesiredVersion} not found in database.", request.DesiredVersion);

            await _auditLogService.AddCustomerLogAsync(
                customer.Id,
                AuditLogLevel.Warning,
                $"Desired version update failed: Release version {request.DesiredVersion} not found.",
                ct);

            return Result.Fail(new ReleaseNotFoundError());
        }

        if (customer.DesiredReleaseId == desiredRelease.Id)
        {
            _logger.LogInformation("Desired version {DesiredVersion} already set for customer {CustomerId}.",
                request.DesiredVersion, request.CustomerId);

            await _auditLogService.AddCustomerLogAsync(
                customer.Id,
                AuditLogLevel.Information,
                $"Desired version update skipped: Release version {request.DesiredVersion} already set.",
                ct);

            return Result.Fail(new DesiredVersionAlreadySetError());
        }

        customer.DesiredReleaseId = desiredRelease.Id;

        await _context.SaveChangesAsync(ct);

        await _auditLogService.AddCustomerLogAsync(
            customer.Id,
            AuditLogLevel.Information,
            $"Desired version updated from {previousReleaseVersion} to {request.DesiredVersion}.",
            ct);

        return Result.Ok();
    }
}