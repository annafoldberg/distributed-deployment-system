using FluentResults;

namespace DeploymentManager.Api.Application.Features.Customers.Errors;

public sealed class CustomerNotFoundError : Error
{
    public CustomerNotFoundError() : base("Customer not found.") {}
}

public sealed class ReleaseNotFoundError : Error
{
    public ReleaseNotFoundError() : base("Release version not found.") {}
}

public sealed class DesiredVersionAlreadySetError : Error
{
    public DesiredVersionAlreadySetError() : base("Desired version already set for customer.") {}
}