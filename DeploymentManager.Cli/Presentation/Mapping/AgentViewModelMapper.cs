using DeploymentManager.Cli.Application.Features.Customers.Models;
using DeploymentManager.Cli.Presentation.Enums;
using DeploymentManager.Cli.Presentation.ViewModels;

namespace DeploymentManager.Cli.Presentation.Mapping;

/// <summary>
/// Maps agent models to agent view models.
/// </summary>
public static class AgentViewModelMapper
{
    /// <summary>
    /// Creates an agent view model from agent data.
    /// </summary>
    /// <param name="agent">Agent to map to view model.</param>
    /// <param name="desiredVersion">
    /// Desired customer version used to calculate agent deployment status.
    /// </param>
    /// <returns>Mapped agent view model.</returns>
    public static AgentViewModel ToViewModel(Agent agent, string desiredVersion)
    {
        return new AgentViewModel
        {
            Id = agent.Id,
            Platform = agent.Platform,
            CurrentVersion = agent.CurrentVersion,
            Status = CalculateDeploymentStatus(agent.CurrentVersion, desiredVersion)
        };
    }

    private static DeploymentStatus CalculateDeploymentStatus(
        string currentVersion,
        string desiredVersion)
    {
        return string.Equals(currentVersion, desiredVersion, StringComparison.OrdinalIgnoreCase)
            ? DeploymentStatus.UpToDate
            : DeploymentStatus.NeedsUpdate;
    }
}