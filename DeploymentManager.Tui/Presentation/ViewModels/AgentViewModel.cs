namespace DeploymentManager.Tui.Presentation.ViewModels;

/// <summary>
/// View model representing an agent.
/// </summary>
public sealed class AgentViewModel
{
    public Guid Id { get; init; }
    public string Platform { get; set; } = string.Empty;
    public string CurrentVersion { get; set; } = string.Empty;
    public DeploymentStatus Status { get; set; }
}