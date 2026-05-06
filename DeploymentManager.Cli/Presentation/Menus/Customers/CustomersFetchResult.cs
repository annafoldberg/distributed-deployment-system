using DeploymentManager.Cli.Presentation.ViewModels;

namespace DeploymentManager.Cli.Presentation.Menus.Customers;

/// <summary>
/// Result of fetching customer view models for customers menu.
/// </summary>
public sealed class CustomersFetchResult
{
    public List<CustomerViewModel>? Customers { get; init; }
    public string? Message { get; init; }
}