namespace DeploymentManager.Tui.Application.Features.Customers.Models;

/// <summary>
/// Represents a result of an operation.
/// </summary>
public sealed class OperationResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    
    public static OperationResult Success() => new()
    {
        IsSuccess = true
    };

    public static OperationResult Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };
}