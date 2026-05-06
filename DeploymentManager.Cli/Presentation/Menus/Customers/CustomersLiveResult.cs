using DeploymentManager.Cli.Presentation.Enums;
using DeploymentManager.Cli.Presentation.ViewModels;

namespace DeploymentManager.Cli.Presentation.Menus.Customers;

/// <summary>
/// Result of loading the customers live view in customers menu.
/// </summary>
public sealed class CustomersLiveResult
{
    public List<CustomerViewModel> Customers { get; init; } = [];
    public CustomersMenuAction Action { get; init; }
}