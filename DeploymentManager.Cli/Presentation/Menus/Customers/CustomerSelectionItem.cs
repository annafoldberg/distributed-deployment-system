using DeploymentManager.Cli.Presentation.ViewModels;

namespace DeploymentManager.Cli.Presentation.Menus.Customers;

/// <summary>
/// Represents either a customer selection or navigation item.
/// </summary>
public sealed class CustomerSelectionItem
{
    public CustomerViewModel? Customer { get; init; }
    public bool IsBack { get; init; }
    public string Label { get; init; } = string.Empty;
}