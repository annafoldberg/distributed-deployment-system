using FluentResults;

namespace DeploymentManager.Api.Application.Features.Common.Errors;

public sealed class AgentNotFoundError : Error
{
    public AgentNotFoundError() : base("Agent not found.") {}
}

public sealed class CustomerNotFoundError : Error
{
    public CustomerNotFoundError() : base("Customer not found.") {}
}

public sealed class ReleaseNotFoundError : Error
{
    public ReleaseNotFoundError() : base("Release version not found.") {}
}