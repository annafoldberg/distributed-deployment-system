using DeploymentManager.Tui.Presentation.ViewModels;

namespace DeploymentManager.Tui.Presentation.Menus.Agents;

/// <summary>
/// Result of loading the agents live view in agents menu.
/// </summary>
public sealed class AgentsLiveResult
{
    public CustomerViewModel Customer { get; init; } = null!;
    public List<AgentViewModel> Agents { get; init; } = null!;
    public AgentsMenuAction Action { get; init; }
}