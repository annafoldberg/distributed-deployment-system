using MediatR;
using FluentResults;
using DeploymentManager.Api.Application.Features.Customers.Dtos;

namespace DeploymentManager.Api.Application.Features.Customers.Queries;

/// <summary>
/// Query for retrieving a single customer.
/// </summary>
public sealed record GetCustomerByIdQuery(Guid CustomerId) : IRequest<Result<CustomerDto>>;