using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using DeploymentManager.Api.Application.Common.Interfaces;
using DeploymentManager.Api.Domain.Entities;
using DeploymentManager.Api.Application.Features.Common.Errors;

namespace DeploymentManager.Api.Application.Features.AuditLogs.Queries;

/// <summary>
/// Handles retrieval of customer audit logs.
/// </summary>
public sealed class GetCustomerAuditLogsQueryHandler : IRequestHandler<GetCustomerAuditLogsQuery, Result<List<AuditLog>>>
{
    private readonly IDeploymentManagerDbContext _context;
    private readonly ILogger<GetCustomerAuditLogsQueryHandler> _logger;

    public GetCustomerAuditLogsQueryHandler(IDeploymentManagerDbContext context, ILogger<GetCustomerAuditLogsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<AuditLog>>> Handle(GetCustomerAuditLogsQuery request, CancellationToken ct)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.PublicId == request.CustomerId, ct);

        if (customer is null)
        {
            _logger.LogWarning("Customer {CustomerId} not found in database.", request.CustomerId);
            return Result.Fail(new CustomerNotFoundError());
        }

        var auditLogs = await _context.AuditLogs
            .Where(l => l.CustomerId == customer.Id)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(ct);

        return Result.Ok(auditLogs);
    }
}