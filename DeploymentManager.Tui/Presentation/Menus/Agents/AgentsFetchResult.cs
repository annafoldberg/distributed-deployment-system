using DeploymentManager.Tui.Presentation.ViewModels;

namespace DeploymentManager.Tui.Presentation.Menus.Agents;

/// <summary>
/// Result of fetching customer and agent view models for agents menu.
/// </summary>
public sealed class AgentsFetchResult
{
    public CustomerViewModel? Customer { get; init; }
    public List<AgentViewModel>? Agents { get; init; }
    public string? Message { get; init; }
}