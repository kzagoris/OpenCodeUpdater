using Spectre.Console;
using OpenCodeUpdater.Models;

namespace upgrade_opencode.Services;

public class SpectreConsoleOutputService : IConsoleOutputService
{
    public bool QuietMode { get; set; }
    public void WriteInfo(string message)
    {
        if (!QuietMode)
        {
            AnsiConsole.MarkupLine($"[blue]{message}[/]");
        }
    }

    public void WriteSuccess(string message)
    {
        if (!QuietMode)
        {
            AnsiConsole.MarkupLine($"[green]{message}[/]");
        }
    }

    public void WriteWarning(string message)
    {
        if (!QuietMode)
        {
            AnsiConsole.MarkupLine($"[yellow]{message}[/]");
        }
    }

    public void WriteError(string message)
    {
        AnsiConsole.MarkupLine($"[red]{message}[/]");
    }

    public void WriteVersionInfo(string label, string version)
    {
        if (!QuietMode)
        {
            AnsiConsole.MarkupLine($"[cyan]{label}:[/] [yellow]{version}[/]");
        }
    }

    public void WriteReleaseNotes(List<ReleaseNote> releaseNotes)
    {
        if (releaseNotes == null || releaseNotes.Count == 0 || QuietMode)
        {
            return;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]═══ Release Notes ═══[/]");
        AnsiConsole.WriteLine();

        foreach (var release in releaseNotes)
        {
            AnsiConsole.MarkupLine($"[bold yellow]{release.Version}[/] - [dim]{release.PublishedAt:yyyy-MM-dd}[/]");

            var sanitizedBody = release.Body.Replace("[", "[[").Replace("]", "]]");
            var lines = sanitizedBody.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines.Take(10))
            {
                var trimmedLine = line.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedLine))
                {
                    AnsiConsole.MarkupLine($"  [grey]{trimmedLine}[/]");
                }
            }

            if (lines.Length > 10)
            {
                AnsiConsole.MarkupLine($"  [dim]... (truncated, see {release.HtmlUrl} for full notes)[/]");
            }

            AnsiConsole.WriteLine();
        }

        AnsiConsole.MarkupLine("[bold cyan]═══════════════════[/]");
        AnsiConsole.WriteLine();
    }

    public IProgressReporter CreateProgressReporter(string description, long totalBytes)
    {
        return new SpectreProgressReporter(description, totalBytes, QuietMode);
    }
}

public class SpectreProgressReporter : IProgressReporter
{
    private readonly string _description;
    private readonly long _totalBytes;
    private readonly bool _quietMode;
    private long _lastDisplayedBytes;
    private const int ProgressBarWidth = 50;

    public SpectreProgressReporter(string description, long totalBytes, bool quietMode)
    {
        _description = description;
        _totalBytes = totalBytes;
        _quietMode = quietMode;
        if (!_quietMode)
        {
            AnsiConsole.MarkupLine($"[green]{_description}[/]");
        }
    }

    public void UpdateProgress(long downloadedBytes, long totalBytes)
    {
        if (_quietMode || downloadedBytes - _lastDisplayedBytes < totalBytes / 100) return;

        _lastDisplayedBytes = downloadedBytes;

        var percentage = (double)downloadedBytes / totalBytes * 100;
        var completedChars = (int)(percentage / 100 * ProgressBarWidth);
        var remainingChars = ProgressBarWidth - completedChars;

        Console.Write("\r");
        AnsiConsole.Markup($"[green]{new string('█', completedChars)}[/]");
        AnsiConsole.Markup($"[grey]{new string('░', remainingChars)}[/]");
        AnsiConsole.Markup($" {percentage:F1}% ([yellow]{downloadedBytes:N0}[/] / [yellow]{totalBytes:N0}[/] bytes)");
    }

    public void Complete()
    {
        if (!_quietMode)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[green]Download completed![/]");
        }
    }

    public void Dispose()
    {
    }
}