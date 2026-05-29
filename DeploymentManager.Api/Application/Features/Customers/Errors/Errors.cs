using FluentResults;

namespace DeploymentManager.Api.Application.Features.Customers.Errors;

public sealed class DesiredVersionAlreadySetError : Error
{
    public DesiredVersionAlreadySetError() : base("Desired version already set for customer.") {}
}