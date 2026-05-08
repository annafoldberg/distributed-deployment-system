using Spectre.Console;
using Spectre.Console.Rendering;

namespace DeploymentManager.Tui.Presentation.Rendering;

/// <summary>
/// Provides shared console layout helpers.
/// </summary>
public static class ConsoleLayout
{
    /// <summary>
    /// Builds standard console layout.
    /// </summary>
    /// <param name="status">Status message displayed at the top of the console layout.</param>
    /// <param name="context">Context displayed before the main layout content.</param>
    /// <param name="content">Main layout content.</param>
    /// <param name="actions">Selectable actions displayed at the bottom of the console layout.</param>
    /// <returns>Configured console layout.</returns>
    public static Layout Build(
        string? status,
        string? context,
        IRenderable? content,
        string? actions)
    {
        var layout = new Layout()
            .SplitRows(
                new Layout("Header").Size(LayoutSizes.Header),
                new Layout("Context").Size(LayoutSizes.Context),
                new Layout("Body"),
                new Layout("Footer").Size(LayoutSizes.Footer)
            );

        layout["Header"].Update(new Markup(status ?? string.Empty));
        layout["Context"].Update(new Markup(context ?? string.Empty));
        layout["Body"].Update(content ?? new Markup(string.Empty));
        layout["Footer"].Update(new Markup(actions ?? string.Empty));

        return layout;
    }

    /// <summary>
    /// Writes fixed header.
    /// </summary>
    /// <param name="status">Status message displayed at the top of the console.</param>
    /// <param name="context">Context displayed below status message.</param>
    public static void WriteHeader(string? status, string? context)
    {
        WriteFixedSection(status, LayoutSizes.Header);
        WriteFixedSection(context, LayoutSizes.Context);
    }

    private static void WriteFixedSection(string? markup, int height)
    {
        AnsiConsole.MarkupLine(markup ?? string.Empty);
        for (var i = 1; i < height; i++) AnsiConsole.WriteLine();
    }
}

// Source: https://spectreconsole.net