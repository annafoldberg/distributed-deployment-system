using FluentValidation;

namespace DeploymentManager.Api.Application.Features.InstallationPackages.Queries;

public class GetInstallationPackageQueryValidator : AbstractValidator<GetInstallationPackageQuery>
{
    public GetInstallationPackageQueryValidator() 
    {
        RuleFor(q => q.AgentId).NotEmpty();
    }
}

// Source: https://docs.fluentvalidation.net/en/latest/aspnet.html