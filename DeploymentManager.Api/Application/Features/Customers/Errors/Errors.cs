using FluentResults;

namespace DeploymentManager.Api.Application.Features.Customers.Errors;

public sealed class CustomerNotFoundError : Error
{
    public CustomerNotFoundError() : base("Customer not found.") {}
}
