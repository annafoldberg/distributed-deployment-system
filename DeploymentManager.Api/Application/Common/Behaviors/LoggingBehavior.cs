using MediatR;

namespace DeploymentManager.Api.Application.Common.Behaviors;

/// <summary>
/// Logs request handling in the MediatR pipeline.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        _logger.LogInformation("Handling {Request}", typeof(TRequest).Name);

        // Execute next step in pipeline
        var response = await next();

        _logger.LogInformation("Handled {Response}", typeof(TResponse).Name);

        return response;
    }
}

// Adapted from MediatR documentation: https://github.com/LuckyPennySoftware/MediatR/wiki/Behaviors