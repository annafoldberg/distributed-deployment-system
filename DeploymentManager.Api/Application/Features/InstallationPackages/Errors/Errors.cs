using FluentResults;

namespace DeploymentManager.Api.Application.Features.InstallationPackages.Errors;

public sealed class AgentNotFoundError : Error
{
    public AgentNotFoundError() : base("Agent not found.") {}
}

public sealed class DesiredReleaseNotSetError : Error
{
    public DesiredReleaseNotSetError() : base("Desired release not set.") {}
}

public sealed class NoUpdateRequiredError : Error
{
    public NoUpdateRequiredError() : base("No update is required.") {}
}

public sealed class PackageNotFoundError : Error
{
    public PackageNotFoundError() : base("Package not found.") {}
}
