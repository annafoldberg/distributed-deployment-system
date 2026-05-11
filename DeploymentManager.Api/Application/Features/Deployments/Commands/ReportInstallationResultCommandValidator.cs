using FluentValidation;

namespace DeploymentManager.Api.Application.Features.Deployments.Commands;

public sealed class ReportInstallationResultCommandValidator : AbstractValidator<ReportInstallationResultCommand>
{
    public ReportInstallationResultCommandValidator() 
    {
        RuleFor(c => c.AgentId).NotEmpty();
        
        When(c => c.Succeeded, () =>
        {
            RuleFor(c => c.ErrorMessage)
                .Null()
                .WithMessage("Error message must be null when installation succeeds.");

            RuleFor(c => c.InstalledVersion)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Installed version is required when installation succeeds.")
                .Matches(@"^\d+\.\d+\.\d+$")
                .WithMessage("Installed version must contain only numbers and dots.");
        });

        When(c => !c.Succeeded, () =>
        {
            RuleFor(c => c.ErrorMessage)
                .NotEmpty()
                .WithMessage("Error message is required when installation fails.");
            
            RuleFor(c => c.InstalledVersion)
                .Null()
                .WithMessage("Installed version must be empty when installation fails.");
        });
    }
}

// Source: https://docs.fluentvalidation.net/en/latest/aspnet.html