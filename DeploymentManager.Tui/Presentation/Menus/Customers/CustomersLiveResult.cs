using DeploymentManager.Tui.Presentation.ViewModels;

namespace DeploymentManager.Tui.Presentation.Menus.Customers;

/// <summary>
/// Result of loading the customers live view in customers menu.
/// </summary>
public sealed class CustomersLiveResult
{
    public List<CustomerViewModel> Customers { get; init; } = [];
    public CustomersMenuAction Action { get; init; }
}