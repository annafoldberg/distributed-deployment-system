using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using DeploymentManager.Api.Application.Common.Interfaces;
using DeploymentManager.Api.Application.Features.Customers.Dtos;
using DeploymentManager.Api.Application.Features.Common.Errors;

namespace DeploymentManager.Api.Application.Features.Customers.Queries;

/// <summary>
/// Handles retrieval of a single customer.
/// </summary>
public sealed class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    private readonly IDeploymentManagerDbContext _context;
    private readonly ILogger<GetCustomerByIdQueryHandler> _logger;

    public GetCustomerByIdQueryHandler(IDeploymentManagerDbContext context, ILogger<GetCustomerByIdQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken ct)
    {
        var customer = await _context.Customers
            .Where(c => c.PublicId == request.CustomerId)
            .Select(c => new CustomerDto
            {
                Id = c.PublicId,
                CompanyName = c.CompanyName,
                DesiredVersion = c.DesiredRelease != null
                    ? c.DesiredRelease.Version
                    : string.Empty,
                Agents = c.Agents.Select(a => new AgentDto
                {
                    Id = a.PublicId,
                    Platform = a.Platform,
                    CurrentVersion = a.Installation != null
                        ? a.Installation.Release.Version
                        : string.Empty
                }).ToList()
            }).FirstOrDefaultAsync(ct);

        if (customer is null)
        {
            _logger.LogWarning("Customer {CustomerId} not found in database.", request.CustomerId);
            return Result.Fail(new CustomerNotFoundError());
        }

        return Result.Ok(customer);
    }
}