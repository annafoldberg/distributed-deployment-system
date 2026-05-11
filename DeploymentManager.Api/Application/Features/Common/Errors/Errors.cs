using FluentResults;

namespace DeploymentManager.Api.Application.Features.Common.Errors;

public sealed class ReleaseNotFoundError : Error
{
    public ReleaseNotFoundError() : base("Release version not found.") {}
}