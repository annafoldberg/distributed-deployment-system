using FluentValidation;

namespace DeploymentManager.Api.Application.Features.Customers.Queries;

public sealed class GetCustomerByIdQueryValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdQueryValidator() 
    {
        RuleFor(q => q.CustomerId).NotEmpty();
    }
}

// Source: https://docs.fluentvalidation.net/en/latest/aspnet.html