namespace DeploymentManager.Api.Domain.Entities;

/// <summary>
/// Represents an installation of the managed software in a customer environment.
/// </summary>
public class Installation
{
    public int Id { get; set; }
    public int AgentId { get; set; }
    public Agent Agent { get; set; } = null!;
    public int ReleaseId { get; set; }
    public Release Release { get; set; } = null!;
}