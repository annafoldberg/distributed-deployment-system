namespace DeploymentManager.Cli.Application.Features.Customers.Models;

/// <summary>
/// Represents customer data.
/// </summary>
public sealed class Customer
{
    public Guid Id { get; init; }
    public string CompanyName { get; set; } = string.Empty;
    public string DesiredVersion { get; set; } = string.Empty;
    public List<Agent> Agents { get; set; } = [];
}