using FluentValidation;

namespace DeploymentManager.Api.Presentation.Middleware;

/// <summary>
/// Maps FluentValidation <see cref="ValidationException"/> to HTTP 400 responses.
/// </summary>
public class ValidationExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ValidationExceptionMiddleware> _logger;

    public ValidationExceptionMiddleware(
        RequestDelegate next,
        ILogger<ValidationExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed");

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var errors = ex.Errors
                .Select(e => new
                {
                    field = e.PropertyName,
                    message = e.ErrorMessage
                });

            await context.Response.WriteAsJsonAsync(new {errors});
        }
    }
}

// Source: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write?view=aspnetcore-10.0