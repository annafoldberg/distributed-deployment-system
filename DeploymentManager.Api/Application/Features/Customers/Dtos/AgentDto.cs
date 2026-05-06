namespace DeploymentManager.Api.Application.Features.Customers.Dtos;

/// <summary>
/// Data transfer object for an agent.
/// </summary>
public sealed class AgentDto
{
    public Guid Id { get; init; }
    public string Platform { get; init; } = string.Empty;
    public string CurrentVersion { get; init; } = string.Empty;
}