using DeploymentManager.Tui.Application.Features.Customers.Models;
using DeploymentManager.Tui.Presentation.ViewModels;

namespace DeploymentManager.Tui.Presentation.Mapping;

/// <summary>
/// Maps customer models to customer view models.
/// </summary>
public static class CustomerViewModelMapper
{
    /// <summary>
    /// Creates a customer view model from customer data.
    /// </summary>
    /// <param name="customer">Customer to map to view model.</param>
    /// <returns>Mapped customer view model.</returns>
    public static CustomerViewModel ToViewModel(Customer customer)
    {
        return new CustomerViewModel
        {
            Id = customer.Id,
            CompanyName = customer.CompanyName,
            DesiredVersion = customer.DesiredVersion,
            CurrentVersionRange = CalculateCurrentVersionRange(customer),
            AgentCount = customer.Agents.Count
        };
    }

    private static string CalculateCurrentVersionRange(Customer customer)
    {
        var versions = customer.Agents
                       .Select(a => a.CurrentVersion)
                       .Where(v => Version.TryParse(v, out _))
                       .Distinct()
                       .OrderBy(v => Version.Parse(v))
                       .ToList();

        return versions.Count switch
        {
            0 => "None",
            1 => versions[0],
            _ => $"{versions.First()} → {versions.Last()}"
        };
    }
}