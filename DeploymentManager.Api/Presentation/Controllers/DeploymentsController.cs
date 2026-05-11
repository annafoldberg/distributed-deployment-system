using MediatR;
using Microsoft.AspNetCore.Mvc;
using DeploymentManager.Api.Application.Features.Deployments.Queries;
using Microsoft.AspNetCore.Authorization;
using DeploymentManager.Api.Presentation.Extensions;
using DeploymentManager.Api.Presentation.Contracts;
using DeploymentManager.Api.Application.Features.Deployments.Commands;
using DeploymentManager.Api.Application.Features.Deployments.Models;

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
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The installation package file when available,
    /// no content when no package should be installed,
    /// otherwise HTTP error response.
    /// </returns>
    [Authorize(Policy = "Agent")]
    [HttpGet("{agentId}/package")]
    public async Task<IActionResult> GetInstallationPackage([FromRoute] Guid agentId, CancellationToken ct)
    {
        var query = new GetInstallationPackageQuery(agentId);
        var result = await _mediator.Send(query, ct);
        
        if (result.IsFailed) return result.ToErrorActionResult();
        
        if (result.Value.Status != InstallationPackageStatus.Available)
            return NoContent();

        var package = result.Value.InstallationPackage;
        if (package is null)
            return StatusCode(StatusCodes.Status500InternalServerError);

        Response.Headers["X-Release-Version"] = package.Version;
        return File(package.Content, package.ContentType, package.FileName);   

    }

    /// <summary>
    /// Reports the result of an installation attempt for an agent.
    /// </summary>
    /// <param name="agentId">Agent identifier.</param>
    /// <param name="result">Installation result.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success if the result is handled, otherwise HTTP error response.</returns>
    [Authorize(Policy = "Agent")]
    [HttpPost("{agentId}/result")]
    public async Task<IActionResult> ReportInstallationResult(
        [FromRoute] Guid agentId,
        [FromBody] InstallationResultDto installationResult,
        CancellationToken ct)
    {
        var command = new ReportInstallationResultCommand(
            agentId,
            installationResult.Succeeded,
            installationResult.InstalledVersion,
            installationResult.ErrorMessage);

        var result = await _mediator.Send(command, ct);
        
        return result.ToActionResult();
    }
}

// Sources:
// MediatR: https://github.com/LuckyPennySoftware/MediatR/wiki
// ControllerBase.File: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.file?view=aspnetcore-10.0