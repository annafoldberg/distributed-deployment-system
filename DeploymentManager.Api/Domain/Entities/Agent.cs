namespace DeploymentManager.Api.Domain.Entities;

/// <summary>
/// Represents an agent in the customer environment that manages deployments of the managed software.
/// </summary>
public class Agent
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public string ApiKeyHash { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
}