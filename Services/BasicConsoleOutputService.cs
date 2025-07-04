using OpenCodeUpdater.Models;

namespace upgrade_opencode.Services;

public class BasicConsoleOutputService : IConsoleOutputService
{
    public bool QuietMode { get; set; }
    public void WriteInfo(string message)
    {
        if (!QuietMode)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }

    public void WriteSuccess(string message)
    {
        if (!QuietMode)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }

    public void WriteWarning(string message)
    {
        if (!QuietMode)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }

    public void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public void WriteVersionInfo(string label, string version)
    {
        if (!QuietMode)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{label}: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(version);
            Console.ResetColor();
        }
    }

    public void WriteReleaseNotes(List<ReleaseNote> releaseNotes)
    {
        if (releaseNotes == null || releaseNotes.Count == 0 || QuietMode)
        {
            return;
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=== Release Notes ===");
        Console.ResetColor();
        Console.WriteLine();

        foreach (var release in releaseNotes)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(release.Version);
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($" - {release.PublishedAt:yyyy-MM-dd}");
            Console.ResetColor();

            var lines = release.Body.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines.Take(10))
            {
                var trimmedLine = line.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedLine))
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"  {trimmedLine}");
                    Console.ResetColor();
                }
            }

            if (lines.Length > 10)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"  ... (truncated, see {release.HtmlUrl} for full notes)");
                Console.ResetColor();
            }

            Console.WriteLine();
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=====================");
        Console.ResetColor();
        Console.WriteLine();
    }

    public IProgressReporter CreateProgressReporter(string description, long totalBytes)
    {
        return new BasicProgressReporter(description, totalBytes, QuietMode);
    }
}

public class BasicProgressReporter : IProgressReporter
{
    private readonly string _description;
    private readonly long _totalBytes;
    private readonly bool _quietMode;
    private long _lastDisplayedBytes;
    private const int ProgressBarWidth = 50;

    public BasicProgressReporter(string description, long totalBytes, bool quietMode)
    {
        _description = description;
        _totalBytes = totalBytes;
        _quietMode = quietMode;
        if (!_quietMode)
        {
            Console.WriteLine(_description);
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
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(new string('█', completedChars));
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(new string('░', remainingChars));
        Console.ResetColor();
        Console.Write($" {percentage:F1}% ({downloadedBytes:N0} / {totalBytes:N0} bytes)");
    }

    public void Complete()
    {
        if (!_quietMode)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Download completed!");
            Console.ResetColor();
        }
    }

    public void Dispose()
    {
    }
}