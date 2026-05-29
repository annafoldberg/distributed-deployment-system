using DeploymentManager.Tui.Presentation.ViewModels;

namespace DeploymentManager.Tui.Presentation.Menus.Agents;

/// <summary>
/// Represents either an agent selection or navigation item.
/// </summary>
public sealed class AgentSelectionItem
{
    public AgentViewModel? Agent { get; init; }
    public bool IsBack { get; init; }
    public string Label { get; init; } = string.Empty;
}