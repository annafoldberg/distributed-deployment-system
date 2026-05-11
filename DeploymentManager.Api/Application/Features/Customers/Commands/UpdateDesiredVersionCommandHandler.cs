using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using DeploymentManager.Api.Application.Common.Interfaces;
using DeploymentManager.Api.Application.Features.Customers.Errors;
using DeploymentManager.Api.Application.Features.Common.Errors;

namespace DeploymentManager.Api.Application.Features.Customers.Commands;

/// <summary>
/// Handles updating the desired version for a customer.
/// </summary>
public sealed class UpdateDesiredVersionCommandHandler : IRequestHandler<UpdateDesiredVersionCommand, Result>
{
    private readonly IDeploymentManagerDbContext _context;
    private readonly ILogger<UpdateDesiredVersionCommandHandler> _logger;

    public UpdateDesiredVersionCommandHandler(IDeploymentManagerDbContext context, ILogger<UpdateDesiredVersionCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateDesiredVersionCommand request, CancellationToken ct)
    {
        var customer = await _context.Customers
            .Where(c => c.PublicId == request.CustomerId)
            .FirstOrDefaultAsync(ct);

        if (customer is null)
        {
            _logger.LogWarning("Customer {CustomerId} not found in database.", request.CustomerId);
            return Result.Fail(new CustomerNotFoundError());
        }

        var desiredRelease = await _context.Releases
            .FirstOrDefaultAsync(r => r.Version == request.DesiredVersion, ct);
            
        if (desiredRelease is null)
        {
            _logger.LogWarning("Release with version {DesiredVersion} not found in database.", request.DesiredVersion);
            return Result.Fail(new ReleaseNotFoundError());
        }

        if (customer.DesiredReleaseId == desiredRelease.Id)
        {
            _logger.LogInformation("Desired version {DesiredVersion} already set for customer {CustomerId}.", request.DesiredVersion, request.CustomerId);
            return Result.Fail(new DesiredVersionAlreadySetError());
        }

        customer.DesiredReleaseId = desiredRelease.Id;
        await _context.SaveChangesAsync(ct);

        return Result.Ok();
    }
}