using FluentValidation;

namespace DeploymentManager.Api.Application.Features.AuditLogs.Queries;

public sealed class GetCustomerAuditLogsQueryValidator : AbstractValidator<GetCustomerAuditLogsQuery>
{
    public GetCustomerAuditLogsQueryValidator() 
    {
        RuleFor(q => q.CustomerId).NotEmpty();
    }
}

// Source: https://docs.fluentvalidation.net/en/latest/aspnet.html