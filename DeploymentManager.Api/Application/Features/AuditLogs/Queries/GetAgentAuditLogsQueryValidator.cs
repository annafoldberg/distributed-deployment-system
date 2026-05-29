using FluentValidation;

namespace DeploymentManager.Api.Application.Features.AuditLogs.Queries;

public sealed class GetAgentAuditLogsQueryValidator : AbstractValidator<GetAgentAuditLogsQuery>
{
    public GetAgentAuditLogsQueryValidator() 
    {
        RuleFor(q => q.AgentId).NotEmpty();
    }
}

// Source: https://docs.fluentvalidation.net/en/latest/aspnet.html