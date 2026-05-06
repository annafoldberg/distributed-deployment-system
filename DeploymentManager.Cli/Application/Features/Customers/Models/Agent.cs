namespace DeploymentManager.Cli.Application.Features.Customers.Models;

/// <summary>
/// Represents agent data.
/// </summary>
public sealed class Agent
{
    public Guid Id { get; init; }
    public string Platform { get; set; } = string.Empty;
    public string CurrentVersion { get; set; } = string.Empty;
}