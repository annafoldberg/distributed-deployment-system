namespace DeploymentManager.Tui.Presentation.ViewModels;

/// <summary>
/// View model representing a customer.
/// </summary>
public sealed class CustomerViewModel
{
    public Guid Id { get; init; }
    public string CompanyName { get; set; } = string.Empty;
    public string DesiredVersion { get; set; } = string.Empty;
    public string CurrentVersionRange { get; set; } = string.Empty;
    public int AgentCount { get; set; }
}