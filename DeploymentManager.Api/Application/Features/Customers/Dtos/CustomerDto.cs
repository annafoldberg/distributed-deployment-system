namespace DeploymentManager.Api.Application.Features.Customers.Dtos;

/// <summary>
/// Data transfer object for a customer.
/// </summary>
public sealed class CustomerDto
{
    public Guid Id { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string DesiredVersion { get; init; } = string.Empty;
    public List<AgentDto> Agents { get; init; } = [];
}