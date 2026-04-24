namespace DeploymentManager.Api.Domain.Entities;

/// <summary>
/// Represents a customer and their desired release of the managed software.
/// </summary>
public class Customer
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int? DesiredReleaseId { get; set; }
    public Release? DesiredRelease { get; set; }
}