using DeploymentManager.Tui.Presentation.ViewModels;

namespace DeploymentManager.Tui.Presentation.Menus.Customers;

/// <summary>
/// Result of fetching customer view models for customers menu.
/// </summary>
public sealed class CustomersFetchResult
{
    public List<CustomerViewModel>? Customers { get; init; }
    public string? Message { get; init; }
}