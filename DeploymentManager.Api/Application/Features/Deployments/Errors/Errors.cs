using FluentResults;

namespace DeploymentManager.Api.Application.Features.Deployments.Errors;

public sealed class AgentNotFoundError : Error
{
    public AgentNotFoundError() : base("Agent not found.") {}
}

public sealed class PackageNotFoundError : Error
{
    public PackageNotFoundError() : base("Package not found.") {}
}
