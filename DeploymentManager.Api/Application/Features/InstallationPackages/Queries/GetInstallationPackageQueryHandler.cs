using DeploymentManager.Api.Application.Features.InstallationPackages.Interfaces;
using DeploymentManager.Api.Application.Features.InstallationPackages.Models;
using MediatR;
using FluentResults;

namespace DeploymentManager.Api.Application.Features.InstallationPackages.Queries;

/// <summary>
/// Handles retrieval of installation packages for an agent.
/// </summary>
public class GetInstallationPackageQueryHandler : IRequestHandler<GetInstallationPackageQuery, Result<InstallationPackage>>
{
    private readonly IPackageProvider _provider;

    public GetInstallationPackageQueryHandler(IPackageProvider provider)
    {
        _provider = provider;
    }

    public async Task<Result<InstallationPackage>> Handle(GetInstallationPackageQuery request, CancellationToken ct)
    {
        // TODO: Find agent (use request)
        // TODO: Find platform (use agent)
        // TODO: Find customer (use agent)
        // TODO: Find desired version (use customer)

        // Temporarily hardcoded platform
        var platform = "osx-arm64";

        // Temporarily hardcoded version
        var version = "1.0.0";

        // Hent package via provider
        var package = await _provider.FetchPackageAsync(platform, version, ct);

        return package == null ? Result.Fail("Package not found") : Result.Ok(package);
    }
}