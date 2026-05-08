using FluentValidation;

namespace DeploymentManager.Api.Application.Features.Customers.Commands;

public sealed class UpdateDesiredVersionCommandValidator : AbstractValidator<UpdateDesiredVersionCommand>
{
    public UpdateDesiredVersionCommandValidator() 
    {
        RuleFor(c => c.CustomerId).NotEmpty();
        RuleFor(c => c.DesiredVersion)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Matches(@"^\d+\.\d+\.\d+$")
            .WithMessage("Desired version must contain only numbers and dots.");
    }
}

// Source: https://docs.fluentvalidation.net/en/latest/aspnet.html