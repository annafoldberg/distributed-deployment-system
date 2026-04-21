using MediatR;
using Microsoft.AspNetCore.Mvc;
using DeploymentManager.Api.Application.Features.InstallationPackages.Queries;
using DeploymentManager.Api.Presentation.Authentication;
using Microsoft.AspNetCore.Authorization;
using DeploymentManager.Api.Presentation.Extensions;

namespace DeploymentManager.Api.Presentation.Controllers;

/// <summary>
/// Exposes deployment endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DeploymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DeploymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves the installation package for an agent.
    /// </summary>
    /// <param name="agentId">Agent identifier.</param>
    /// <returns>The installation package file if retrieval succeeds, otherwise HTTP error response.</returns>
    [Authorize(AuthenticationSchemes = AgentApiKeyAuthenticationHandler.SchemeName)]
    [HttpGet("{agentId}/package")]
    public async Task<IActionResult> GetInstallationPackage([FromRoute] Guid agentId)
    {
        var query = new GetInstallationPackageQuery(agentId);
        var result = await _mediator.Send(query);
        
        if (result.IsFailed) return result.ToErrorActionResult();

        var package = result.Value;
        return File(package.Content, package.ContentType, package.FileName);
    }
}

// Sources:
// MediatR: https://github.com/LuckyPennySoftware/MediatR/wiki
// ControllerBase.File: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.file?view=aspnetcore-10.0