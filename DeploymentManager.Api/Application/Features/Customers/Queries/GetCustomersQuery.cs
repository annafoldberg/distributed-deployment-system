using MediatR;
using FluentResults;
using DeploymentManager.Api.Application.Features.Customers.Dtos;

namespace DeploymentManager.Api.Application.Features.Customers.Queries;

/// <summary>
/// Query for retrieving customers.
/// </summary>
public sealed record GetCustomersQuery() : IRequest<Result<List<CustomerDto>>>;