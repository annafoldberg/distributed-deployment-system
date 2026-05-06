using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using DeploymentManager.Api.Application.Common.Interfaces;
using DeploymentManager.Api.Application.Features.Customers.Dtos;

namespace DeploymentManager.Api.Application.Features.Customers.Queries;

/// <summary>
/// Handles retrieval of customers.
/// </summary>
public sealed class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, Result<List<CustomerDto>>>
{
    private readonly IDeploymentManagerDbContext _context;
    private readonly ILogger<GetCustomersQueryHandler> _logger;

    public GetCustomersQueryHandler(IDeploymentManagerDbContext context, ILogger<GetCustomersQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<CustomerDto>>> Handle(GetCustomersQuery request, CancellationToken ct)
    {
        var customers = await _context.Customers
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
            }).ToListAsync(ct);

        if (customers.Count == 0)
            _logger.LogInformation("No customers found in database.");

        return Result.Ok(customers);
    }
}