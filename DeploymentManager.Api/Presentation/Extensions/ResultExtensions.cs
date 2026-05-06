using DeploymentManager.Api.Application.Features.Customers.Errors;
using DeploymentManager.Api.Application.Features.InstallationPackages.Errors;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace DeploymentManager.Api.Presentation.Extensions;

/// <summary>
/// Provides extensions for converting results into HTTP action results.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts result into HTTP action result.
    /// Returns 200 OK with the value on success, otherwise maps errors to HTTP responses.
    /// </summary>
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess) return new OkObjectResult(result.Value);
        
        return result.ToErrorActionResult();
    }

    /// <summary>
    /// Converts failed result into HTTP action result based on application error.
    /// </summary>
    public static IActionResult ToErrorActionResult<T>(this Result<T> result)
    {
        var errorMessages = result.Errors.Select(e => e.Message);

        if (result.HasError<CustomerNotFoundError>())
            return new NotFoundObjectResult(errorMessages);

        if (result.HasError<AgentNotFoundError>())
            return new NotFoundObjectResult(errorMessages);

        if (result.HasError<DesiredReleaseNotSetError>())
            return new BadRequestObjectResult(errorMessages);

        if (result.HasError<NoUpdateRequiredError>())
            return new BadRequestObjectResult(errorMessages);

        if (result.HasError<PackageNotFoundError>())
            return new NotFoundObjectResult(errorMessages);

        return new BadRequestObjectResult(errorMessages);
    }
}