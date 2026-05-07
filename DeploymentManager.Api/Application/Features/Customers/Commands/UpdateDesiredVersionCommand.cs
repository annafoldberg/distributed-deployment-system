using MediatR;
using FluentResults;

namespace DeploymentManager.Api.Application.Features.Customers.Commands;

/// <summary>
/// Command for updating the desired version for a customer.
/// </summary>
public sealed record UpdateDesiredVersionCommand(Guid CustomerId, string DesiredVersion) : IRequest<Result>;