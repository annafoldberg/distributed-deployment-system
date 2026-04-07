using FluentValidation;
using MediatR;

namespace DeploymentManager.Api.Application.Common.Behaviors;

/// <summary>
/// Validates requests in the MediatR pipeline.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        // Create validation context from request for validators
        var context = new ValidationContext<TRequest>(request);

        // Execute all validators for request
        var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, ct)));

        // Extract validation failures
        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();
        
        // Throw on validation errors
        if (failures.Any())
        {
            _logger.LogWarning(
                "Validation failed for {Request}: {@Failures}",
                typeof(TRequest).Name,
                failures);
            throw new ValidationException(failures);
        }

        // Execute next step in pipeline
        return await next();
    }
}

// Sources:
// MediatR behaviors: https://github.com/LuckyPennySoftware/MediatR/wiki/Behaviors
// Highly inspired by ValidationBehavior example: https://the-runtime.dev/articles/cqrs-with-mediatr/