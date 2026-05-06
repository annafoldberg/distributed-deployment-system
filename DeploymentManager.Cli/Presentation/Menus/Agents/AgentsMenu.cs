using DeploymentManager.Cli.Application.Features.Customers;
using DeploymentManager.Cli.Presentation.Enums;
using DeploymentManager.Cli.Presentation.Mapping;
using DeploymentManager.Cli.Presentation.Rendering;
using DeploymentManager.Cli.Presentation.ViewModels;
using Spectre.Console;

namespace DeploymentManager.Cli.Presentation.Menus.Agents;

/// <summary>
/// Displays the agents menu and handles back navigation to customers.
/// </summary>
public sealed class AgentsMenu
{
    private readonly ICustomersService _customersService;

    public AgentsMenu(ICustomersService customersService)
    {
        _customersService = customersService;
    }

    /// <summary>
    /// Shows the agents menu.
    /// </summary>
    /// <param name="customerId">ID of the customer to display agents for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The selected agents menu action.</returns>
    public async Task<AgentsMenuAction> ShowAsync(Guid customerId, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            AnsiConsole.Clear();
            var action = await ShowAgentsLiveAsync(customerId, ct);
            
            if (action == AgentsMenuAction.Quit || action == AgentsMenuAction.Back)
            {
                AnsiConsole.Clear();
                return action;
            }
        }

        return AgentsMenuAction.Quit;
    }

    private async Task<AgentsMenuAction> ShowAgentsLiveAsync(Guid customerId, CancellationToken ct)
    {
        var action = AgentsMenuAction.None;
        var lastFetch = DateTimeOffset.MinValue;
        var customer = new CustomerViewModel();
        var agents = new List<AgentViewModel>();
        var refreshInterval = TimeSpan.FromSeconds(10);
        var message = "Loading agents...";

        await AnsiConsole.Live(BuildLiveView(customer, agents, message, lastFetch, refreshInterval))
            .Overflow(VerticalOverflow.Visible)
            .StartAsync(async ctx =>
            {
                while (!ct.IsCancellationRequested && action == AgentsMenuAction.None)
                {
                    if (DateTimeOffset.UtcNow - lastFetch >= refreshInterval)
                    {
                        var fetchResult = await GetAgentViewModelsAsync(customerId, ct);
                        if (fetchResult.Customer is not null) customer = fetchResult.Customer;
                        if (fetchResult.Agents is not null) agents = fetchResult.Agents;
                        
                        lastFetch = DateTimeOffset.UtcNow;
                        message = fetchResult.Message;
                    }
                    else message = null;

                    ctx.UpdateTarget(BuildLiveView(customer, agents, message, lastFetch, refreshInterval));
                    ctx.Refresh();

                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(intercept: true).Key;
                        if (key == ConsoleKey.B) action = AgentsMenuAction.Back;
                        if (key == ConsoleKey.Q) action = AgentsMenuAction.Quit;
                    }

                    await Task.Delay(100, ct);
                }
            });

        return action;
    }

    private Layout BuildLiveView(
        CustomerViewModel customer,
        IEnumerable<AgentViewModel> agents,
        string? message,
        DateTimeOffset lastFetch,
        TimeSpan refreshInterval)
    {
        var nextUpdateIn = lastFetch == DateTimeOffset.MinValue
            ? 0
            : Math.Max(0, (int)(refreshInterval - (DateTimeOffset.UtcNow - lastFetch)).TotalSeconds);

        var status = message is not null
            ? message
            : $"[Red3_1]•[/] [Grey70]Live data · next refresh in {nextUpdateIn}s[/]";

        var context = $"{customer.CompanyName} · Desired version: {customer.DesiredVersion}";
        var content = BuildAgentsTable(agents);
        var actions = "[DodgerBlue2]<B>[/] Back  [dim]<Q>[/] Quit";

        return ConsoleLayout.Build(status, context, content, actions);
    }

    private Table BuildAgentsTable(IEnumerable<AgentViewModel> agents)
    {
        var agentsTable = new Table()
                .Border(TableBorder.None)   
                .AddColumn("Agent ID".PadRight(38))
                .AddColumn("Current Version".PadRight(18))
                .AddColumn("Status".PadRight(18))
                .AddColumn("Platform");

        foreach (var agent in agents)
        {
            var agentStatus = agent.Status switch
            {
                DeploymentStatus.NeedsUpdate => $"[Red3_1]! Needs update[/]",
                DeploymentStatus.UpToDate => $"[Green3_1]✓ Up to date[/]",
                _ => $"[Gold1]? Unknown[/]"
            };

            agentsTable.AddRow(
                agent.Id.ToString(),
                agent.CurrentVersion,
                agentStatus,
                agent.Platform);
        }
        return agentsTable;
    }

    private async Task<AgentsFetchResult> GetAgentViewModelsAsync(Guid customerId, CancellationToken ct)
    {
        var customer = await _customersService.GetCustomerByIdAsync(customerId, ct);

        if (customer is null)
            return new AgentsFetchResult{ Message = "[Red3_1]Failed to load customer.[/]" };

        var customerViewModel = CustomerViewModelMapper.ToViewModel(customer);
        
        if (customer.Agents.Count == 0)
            return new AgentsFetchResult{ Customer = customerViewModel, Agents = [], Message = "[Gold1]No agents found for customer.[/]"};

        var agentViewModels = customer.Agents.Select(a =>
            AgentViewModelMapper.ToViewModel(a, customerViewModel.DesiredVersion))
            .ToList();

        if (agentViewModels.Count == 0)
        {
            return new AgentsFetchResult{ Customer = customerViewModel, Agents = [], Message = "[Red3_1]Failed to map agents.[/]" };
        }

        return new AgentsFetchResult{ Customer = customerViewModel, Agents = agentViewModels };
    }
}

// Sources:
// Spectre.Console: https://spectreconsole.net
// Live: https://spectreconsole.net/console/live/live-display