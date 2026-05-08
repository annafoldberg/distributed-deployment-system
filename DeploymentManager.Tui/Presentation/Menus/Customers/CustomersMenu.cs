using DeploymentManager.Tui.Application.Features.Customers;
using DeploymentManager.Tui.Presentation.Enums;
using DeploymentManager.Tui.Presentation.Mapping;
using DeploymentManager.Tui.Presentation.Menus.Agents;
using DeploymentManager.Tui.Presentation.Rendering;
using DeploymentManager.Tui.Presentation.ViewModels;
using Spectre.Console;

namespace DeploymentManager.Tui.Presentation.Menus.Customers;

/// <summary>
/// Displays the customers menu and handles navigation to agent details.
/// </summary>
public sealed class CustomersMenu
{
    private readonly ICustomersService _customersService;
    private readonly AgentsMenu _agentsMenu;

    public CustomersMenu(ICustomersService customersService, AgentsMenu agentsMenu)
    {
        _customersService = customersService;
        _agentsMenu = agentsMenu;
    }

    /// <summary>
    /// Shows the customers menu.
    /// </summary>
    public async Task ShowAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            AnsiConsole.Clear();
            var liveResult = await ShowCustomersLiveAsync(ct);
            
            if (liveResult.Action == CustomersMenuAction.Quit)
            {
                AnsiConsole.Clear();
                return;
            }

            if (liveResult.Action == CustomersMenuAction.Select)
            {
                AnsiConsole.Clear();
                var selection = ShowCustomerSelection(liveResult.Customers);

                if (selection.IsBack || selection.Customer is null) continue;

                var agentsMenuAction = await _agentsMenu.ShowAsync(selection.Customer.Id, ct);
                if (agentsMenuAction == AgentsMenuAction.Quit) return;
            }
        }
    }

    private async Task<CustomersLiveResult> ShowCustomersLiveAsync(CancellationToken ct)
    {
        var action = CustomersMenuAction.None;
        var customers = new List<CustomerViewModel>();
        var lastFetch = DateTimeOffset.MinValue;
        var refreshInterval = TimeSpan.FromSeconds(10);
        var message = "Loading customers...";

        await AnsiConsole.Live(BuildLiveView(customers, message, lastFetch, refreshInterval))
            .Overflow(VerticalOverflow.Visible)
            .StartAsync(async ctx =>
            {
                while (!ct.IsCancellationRequested && action == CustomersMenuAction.None)
                {
                    if (DateTimeOffset.UtcNow - lastFetch >= refreshInterval)
                    {
                        var fetchResult = await GetCustomerViewModelsAsync(ct);
                        if (fetchResult.Customers is not null) customers = fetchResult.Customers;
                        
                        lastFetch = DateTimeOffset.UtcNow;
                        message = fetchResult.Message;
                    }
                    else message = null;

                    ctx.UpdateTarget(BuildLiveView(customers, message, lastFetch, refreshInterval));
                    ctx.Refresh();

                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(intercept: true).Key;
                        if (key == ConsoleKey.S && customers.Count > 0) action = CustomersMenuAction.Select;
                        if (key == ConsoleKey.Q) action = CustomersMenuAction.Quit;
                    }

                    await Task.Delay(100, ct);
                }
            });

        return new CustomersLiveResult
        {
            Customers = customers,
            Action = action
        };
    }

    private Layout BuildLiveView(
        IEnumerable<CustomerViewModel> customers,
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

        var context = "Customers";
        var content = BuildCustomersTable(customers);
        var actions = "[DeepPink3_1]<S>[/] Select  [dim]<Q>[/] Quit";

        return ConsoleLayout.Build(status, context, content, actions);
    }

    private Table BuildCustomersTable(IEnumerable<CustomerViewModel> customers)
    {
        var customersTable = new Table()
                .Border(TableBorder.None)   
                .AddColumn("Company".PadRight(20))
                .AddColumn("Agents".PadRight(10))
                .AddColumn("Current Version".PadRight(20))
                .AddColumn("Desired Version");

        foreach (var customer in customers)
        {
            var currentVersionRange = string.IsNullOrWhiteSpace(customer.CurrentVersionRange)
                                      ? "[Grey70]–[/]"
                                      : customer.CurrentVersionRange;

            var desiredVersion = string.IsNullOrWhiteSpace(customer.DesiredVersion)
                                 ? "[Grey70]–[/]"
                                 : customer.DesiredVersion;
            
            customersTable.AddRow(
                customer.CompanyName,
                customer.AgentCount.ToString(),
                currentVersionRange,
                desiredVersion);
        }

        return customersTable;
    }

    private CustomerSelectionItem ShowCustomerSelection(IEnumerable<CustomerViewModel> customers)
    {
        var status = "[Grey53]•[/] [Grey70]Live updates paused[/]";
        var context = "[Purple_2]<↑ ↓>[/] Navigate  [Green3_1]<Enter>[/] Select";

        ConsoleLayout.WriteHeader(status, context);

        AnsiConsole.MarkupLine(
            $"{"Company".PadLeft(2).PadRight(20)}" +
            $"{"Agents".PadRight(10)}" +
            $"{"Current Version".PadRight(20)}" +
            $"{"Desired Version"}");

        var items = customers.Select(customer => new CustomerSelectionItem
        {
            Customer = customer,
            Label =
                $"{customer.CompanyName.PadRight(20)}" +
                $"{customer.AgentCount.ToString().PadRight(10)}" +
                $"{(string.IsNullOrWhiteSpace(customer.CurrentVersionRange)
                    ? "[Grey70]" + "–".PadRight(20) + "[/]"
                    : customer.CurrentVersionRange.PadRight(20))}" +
                $"{(string.IsNullOrWhiteSpace(customer.DesiredVersion)
                    ? "[Grey70]–[/]"
                    : customer.DesiredVersion)}"
        }).ToList();

        items.Add(new CustomerSelectionItem
        {
            IsBack = true,
            Label = "[DodgerBlue2]← Back[/]"
        });

        var prompt = new SelectionPrompt<CustomerSelectionItem>()
            .PageSize(10)
            .UseConverter(item => item.Label)
            .HighlightStyle(new Style(Color.LightSeaGreen))
            .AddChoices(items);

        return AnsiConsole.Prompt(prompt);
    }

    private async Task<CustomersFetchResult> GetCustomerViewModelsAsync(CancellationToken ct)
    {
        var customers = await _customersService.GetCustomersAsync(ct);

        if (customers is null)
            return new CustomersFetchResult{ Message = "[Red3_1]Failed to load customers.[/]" };

        if (customers.Count == 0)
            return new CustomersFetchResult{ Customers = [], Message = "[Gold1]No customers found.[/]" };

        var customerViewModels = customers.Select(
            CustomerViewModelMapper.ToViewModel)
            .ToList();

        if (customerViewModels.Count == 0)
            return new CustomersFetchResult{ Customers = [], Message = "[Red3_1]Failed to map customers.[/]" };

        return new CustomersFetchResult{ Customers = customerViewModels };
    }
}

// Sources:
// Spectre.Console: https://spectreconsole.net
// Live: https://spectreconsole.net/console/live/live-display
// SelectionPrompt: https://spectreconsole.net/console/prompts/selection-prompt