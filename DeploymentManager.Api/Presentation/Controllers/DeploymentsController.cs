using MediatR;
using Microsoft.AspNetCore.Mvc;
using DeploymentManager.Api.Application.Features.InstallationPackages.Queries;

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
    /// <returns>The installation package file if found, otherwise 404.</returns>
    [HttpGet("{agentId}/package")]
    public async Task<IActionResult> GetInstallationPackage([FromRoute] Guid agentId)
    {
        var query = new GetInstallationPackageQuery(agentId);
        var result = await _mediator.Send(query);

        if (result.IsFailed)
            return NotFound(result.Errors.Select(e => e.Message));

        var package = result.Value;
        return File(package.Content, package.ContentType, package.FileName);
    }
}

// Sources:
// MediatR: https://github.com/LuckyPennySoftware/MediatR/wiki
// ControllerBase.File: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.file?view=aspnetcore-10.0