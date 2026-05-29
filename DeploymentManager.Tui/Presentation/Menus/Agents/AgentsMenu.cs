using System.Text.RegularExpressions;
using DeploymentManager.Tui.Application.Features.Customers;
using DeploymentManager.Tui.Presentation.Mapping;
using DeploymentManager.Tui.Presentation.Menus.AuditLogs;
using DeploymentManager.Tui.Presentation.Rendering;
using DeploymentManager.Tui.Presentation.ViewModels;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace DeploymentManager.Tui.Presentation.Menus.Agents;

/// <summary>
/// Displays the agents menu, enables update of desired version for customer,
/// and handles back navigation to customers.
/// </summary>
public sealed class AgentsMenu
{
    private readonly ICustomersService _customersService;
    private readonly AuditLogsMenu _auditLogsMenu;

    public AgentsMenu(ICustomersService customersService, AuditLogsMenu auditLogsMenu)
    {
        _customersService = customersService;
        _auditLogsMenu = auditLogsMenu;
    }

    /// <summary>
    /// Shows the agents menu.
    /// </summary>
    /// <param name="customerId">Identifier of the customer to display agents for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The selected agents menu action.</returns>
    public async Task<AgentsMenuAction> ShowAsync(Guid customerId, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            AnsiConsole.Clear();
            var liveResult = await ShowAgentsLiveAsync(customerId, ct);
            
            if (liveResult.Action == AgentsMenuAction.UpdateDesiredVersion)
            {
                AnsiConsole.Clear();
                await ShowUpdateDesiredVersionPromptAsync(customerId, liveResult.Customer, ct);
            }

            if (liveResult.Action == AgentsMenuAction.ViewCustomerAuditLogs)
            {
                AnsiConsole.Clear();

                var auditLogsMenuAction = await _auditLogsMenu.ShowAsync(
                    customerId, AuditLogSource.Customer, liveResult.Customer.CompanyName, ct);

                if (auditLogsMenuAction == AuditLogsMenuAction.Quit) return AgentsMenuAction.Quit;
            }

            if (liveResult.Action == AgentsMenuAction.ViewAgentAuditLogs)
            {
                AnsiConsole.Clear();
                var selection = ShowAgentSelection(liveResult.Agents);

                if (selection.IsBack || selection.Agent is null) continue;

                var auditLogsMenuAction = await _auditLogsMenu.ShowAsync(
                    selection.Agent.Id, AuditLogSource.Agent, liveResult.Customer.CompanyName, ct);

                if (auditLogsMenuAction == AuditLogsMenuAction.Quit) return AgentsMenuAction.Quit;
            }

            if (liveResult.Action == AgentsMenuAction.Quit || liveResult.Action == AgentsMenuAction.Back)
            {
                AnsiConsole.Clear();
                return liveResult.Action;
            }
        }

        return AgentsMenuAction.Quit;
    }

    private async Task<AgentsLiveResult> ShowAgentsLiveAsync(Guid customerId, CancellationToken ct)
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
                        if (key == ConsoleKey.D) action = AgentsMenuAction.UpdateDesiredVersion;
                        if (key == ConsoleKey.C) action = AgentsMenuAction.ViewCustomerAuditLogs;
                        if (key == ConsoleKey.A && agents.Count > 0) action = AgentsMenuAction.ViewAgentAuditLogs;
                        if (key == ConsoleKey.B) action = AgentsMenuAction.Back;
                        if (key == ConsoleKey.Q) action = AgentsMenuAction.Quit;
                    }

                    await Task.Delay(100, ct);
                }
            });

        return new AgentsLiveResult
        {
            Customer = customer,
            Agents = agents,
            Action = action
        };
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

        var status = $"[Red3_1]•[/] [Grey70]Live data · next refresh in {nextUpdateIn}s[/]";

        var desiredVersion = string.IsNullOrWhiteSpace(customer.DesiredVersion)
                             ? "[Grey70]None[/]"
                             : customer.DesiredVersion;

        var context = $"{customer.CompanyName} · Desired version: {desiredVersion}";
        IRenderable content = !agents.Any() && message is not null ? new Markup(message) : BuildAgentsTable(agents);
        var actions = "[Gold1]<D>[/] Update Desired Version  [Purple_2]<C>[/] Customer Logs  [DeepPink3_1]<A>[/] Agent Logs  [DodgerBlue2]<B>[/] Back  [dim]<Q>[/] Quit";

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

            var currentVersion = string.IsNullOrWhiteSpace(agent.CurrentVersion)
                                 ? "[Grey70]–[/]"
                                 : agent.CurrentVersion;

            agentsTable.AddRow(
                agent.Id.ToString(),
                currentVersion,
                agentStatus,
                agent.Platform);
        }
        return agentsTable;
    }

    private async Task ShowUpdateDesiredVersionPromptAsync(Guid customerId, CustomerViewModel customer, CancellationToken ct)
    {
        var desiredVersion = string.IsNullOrWhiteSpace(customer.DesiredVersion)
                             ? "[Grey70]None[/]"
                             : customer.DesiredVersion;

        var status = "[Grey53]•[/] [Grey70]Live updates paused[/]";
        var context = $"{customer.CompanyName} · Desired version: {desiredVersion}\n" +
                      $"Type [Purple_2]'exit'[/] to cancel  [Green3_1]<Enter>[/] Confirm";

        ConsoleLayout.WriteHeader(status, context);

        var prompt = new TextPrompt<string>($"Enter new [LightSeaGreen]desired version[/]:")
            .Validate(input =>
            {
                if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
                    return ValidationResult.Success();
                if (Regex.IsMatch(input, @"^\d+\.\d+\.\d+$"))
                    return ValidationResult.Success();
                return ValidationResult.Error("[Red3_1]Version must follow format: major.minor.patch[/]");
            });

        var input = AnsiConsole.Prompt(prompt);

        if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase)) return;

        var result = await _customersService.UpdateDesiredVersionAsync(customerId, input, ct);
        
        if (result.IsSuccess)
        {
            AnsiConsole.MarkupLine("\n[Green3_1]Desired version updated successfully.[/]");
            await Task.Delay(2000, ct);
            return;
        }

        AnsiConsole.MarkupLine($"[Red3_1]{result.ErrorMessage}[/]\n\n" +
            "[Grey70]Press [Green3_1]<Enter>[/] to continue.[/]");
        Console.ReadKey();
    }

    private AgentSelectionItem ShowAgentSelection(IEnumerable<AgentViewModel> agents)
    {
        var status = "[Grey53]•[/] [Grey70]Live updates paused[/]";
        var context = "[Purple_2]<↑ ↓>[/] Navigate  [Green3_1]<Enter>[/] Select";

        ConsoleLayout.WriteHeader(status, context);

        var selectionIndent = "  ";

        AnsiConsole.MarkupLine(
            selectionIndent +
            $"{"Agent ID".PadRight(38)}" +
            $"{"Current Version".PadRight(18)}" +
            $"{"Status".PadRight(18)}" +
            $"{"Platform"}");

        var items = agents.Select(agent =>
        {
            var agentStatus = agent.Status switch
            {
                DeploymentStatus.NeedsUpdate => $"[Red3_1]{"! Needs update".PadRight(18)}[/]",
                DeploymentStatus.UpToDate => $"[Green3_1]{"✓ Up to date".PadRight(18)}[/]",
                _ => $"[Gold1]{"? Unknown".PadRight(18)}[/]"
            };

            return new AgentSelectionItem
            {
                Agent = agent,
                Label =
                    $"{agent.Id.ToString().PadRight(38)}" +
                    $"{agent.CurrentVersion.PadRight(18)}" +
                    $"{agentStatus}" +
                    $"{agent.Platform}"
            };
        }).ToList();

        items.Add(new AgentSelectionItem
        {
            IsBack = true,
            Label = "[DodgerBlue2]← Back[/]"
        });

        var prompt = new SelectionPrompt<AgentSelectionItem>()
            .PageSize(10)
            .UseConverter(item => item.Label)
            .HighlightStyle(new Style(Color.LightSeaGreen))
            .AddChoices(items);

        return AnsiConsole.Prompt(prompt);
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
// TextPrompt: https://spectreconsole.net/console/prompts/text-prompt