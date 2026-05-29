using DeploymentManager.Tui.Application.Features.AuditLogs;
using DeploymentManager.Tui.Application.Features.AuditLogs.Enums;
using DeploymentManager.Tui.Presentation.Mapping;
using DeploymentManager.Tui.Presentation.Rendering;
using DeploymentManager.Tui.Presentation.ViewModels;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace DeploymentManager.Tui.Presentation.Menus.AuditLogs;

/// <summary>
/// Displays the audit logs for a given source and handles back navigation to source menu.
/// </summary>
public sealed class AuditLogsMenu
{
    private readonly IAuditLogsService _auditLogsService;

    public AuditLogsMenu(IAuditLogsService auditLogsService)
    {
        _auditLogsService = auditLogsService;
    }

    /// <summary>
    /// Shows the audit logs menu.
    /// </summary>
    /// <param name="sourceId">Identifier of the source to display audit logs for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The selected audit logs menu action.</returns>
    public async Task<AuditLogsMenuAction> ShowAsync(Guid sourceId, AuditLogSource source, string companyName, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            AnsiConsole.Clear();

            var action = await ShowAuditLogsLiveAsync(sourceId, source, companyName, ct);
            
            if (action == AuditLogsMenuAction.Quit || action == AuditLogsMenuAction.Back)
            {
                AnsiConsole.Clear();
                return action;
            }
        }

        return AuditLogsMenuAction.Quit;
    }

    private async Task<AuditLogsMenuAction> ShowAuditLogsLiveAsync(Guid sourceId, AuditLogSource source, string companyName, CancellationToken ct)
    {
        var action = AuditLogsMenuAction.None;
        var lastFetch = DateTimeOffset.MinValue;
        var auditLogs = new List<AuditLogViewModel>();
        var refreshInterval = TimeSpan.FromSeconds(10);
        var message = "Loading audit logs...";

        await AnsiConsole.Live(BuildLiveView(source, companyName, auditLogs, message, lastFetch, refreshInterval))
            .Overflow(VerticalOverflow.Visible)
            .StartAsync(async ctx =>
            {
                while (!ct.IsCancellationRequested && action == AuditLogsMenuAction.None)
                {
                    if (DateTimeOffset.UtcNow - lastFetch >= refreshInterval)
                    {
                        var fetchResult = await GetAuditLogViewModelsAsync(sourceId, source, ct);
                        if (fetchResult.AuditLogs is not null) auditLogs = fetchResult.AuditLogs;
                        
                        lastFetch = DateTimeOffset.UtcNow;
                        message = fetchResult.Message;
                    }

                    ctx.UpdateTarget(BuildLiveView(source, companyName, auditLogs, message, lastFetch, refreshInterval));
                    ctx.Refresh();

                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(intercept: true).Key;
                        if (key == ConsoleKey.B) action = AuditLogsMenuAction.Back;
                        if (key == ConsoleKey.Q) action = AuditLogsMenuAction.Quit;
                    }

                    await Task.Delay(100, ct);
                }
            });

        return action;
    }

    private Layout BuildLiveView(
        AuditLogSource source,
        string companyName,
        IEnumerable<AuditLogViewModel> auditLogs,
        string? message,
        DateTimeOffset lastFetch,
        TimeSpan refreshInterval)
    {
        var nextUpdateIn = lastFetch == DateTimeOffset.MinValue
            ? 0
            : Math.Max(0, (int)(refreshInterval - (DateTimeOffset.UtcNow - lastFetch)).TotalSeconds);

        var status =  $"[Red3_1]•[/] [Grey70]Live data · next refresh in {nextUpdateIn}s[/]";
        var context = $"{source} audit logs for {companyName}";
        IRenderable content = !auditLogs.Any() && message is not null
                              ? new Markup(message)
                              : BuildAuditLogsTable(auditLogs);
        var actions = "[DodgerBlue2]<B>[/] Back  [dim]<Q>[/] Quit";

        return ConsoleLayout.Build(status, context, content, actions);
    }

    private Table BuildAuditLogsTable(IEnumerable<AuditLogViewModel> auditLogs)
    {
        var auditLogsTable = new Table()
                .Border(TableBorder.None)   
                .AddColumn("Timestamp".PadRight(32))
                .AddColumn("Level".PadRight(10))
                .AddColumn("Event");

        foreach (var auditLog in auditLogs)
        {
            var auditLogLevel = auditLog.Level switch
            {
                AuditLogLevel.Warning => "[Gold1]WARN[/]",
                AuditLogLevel.Information => "[Grey70]INFO[/]",
                _ => "[Grey70]UNKNOWN[/]"
            };

            auditLogsTable.AddRow(
                auditLog.CreatedAt.ToString(),
                auditLogLevel,
                auditLog.Message);
        }

        return auditLogsTable;
    }

    private async Task<AuditLogsFetchResult> GetAuditLogViewModelsAsync(Guid sourceId, AuditLogSource source, CancellationToken ct)
    {
        var auditLogs = source switch
        {
            AuditLogSource.Customer => await _auditLogsService.GetCustomerAuditLogsAsync(sourceId, ct),
            AuditLogSource.Agent => await _auditLogsService.GetAgentAuditLogsAsync(sourceId, ct),
            _ => null
        };

        if (auditLogs is null)
            return new AuditLogsFetchResult{ Message = $"[Red3_1]Failed to load audit logs for {source.ToString().ToLowerInvariant()}.[/]"};

        if (auditLogs.Count == 0)
            return new AuditLogsFetchResult{ AuditLogs = [], Message = $"[Grey70]No audit logs found for {source.ToString().ToLowerInvariant()}.[/]"};

        var auditLogViewModels = auditLogs.Select(
            AuditLogViewModelMapper.ToViewModel)
            .ToList();

        if (auditLogViewModels.Count == 0)
        {
            return new AuditLogsFetchResult{ AuditLogs = [], Message = "[Red3_1]Failed to map audit logs.[/]" };
        }

        return new AuditLogsFetchResult{ AuditLogs = auditLogViewModels };
    }
}

// Sources:
// Spectre.Console: https://spectreconsole.net
// Live: https://spectreconsole.net/console/live/live-display