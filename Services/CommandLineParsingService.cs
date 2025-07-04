using OpenCodeUpdater.Models;

namespace OpenCodeUpdater.Services;

public class CommandLineParsingService
{
    public async Task<int> ParseAndExecuteAsync(string[] args, OpenCodeUpdater updater)
    {
        var options = ParseOptions(args);
        
        if (args.Contains("--help") || args.Contains("-h"))
        {
            ShowHelp();
            return 0;
        }

        if (args.Contains("--version"))
        {
            ShowVersion();
            return 0;
        }

        bool result = await updater.UpdateAsync(
            forceUpdate: options.ForceUpdate,
            skipReleaseNotes: options.SkipReleaseNotes,
            quietMode: options.QuietMode,
            customPath: options.CustomPath);
        
        return result ? 0 : 1;
    }

    public CommandLineOptions ParseOptions(string[] args)
    {
        var options = new CommandLineOptions();
        
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "--force":
                case "-f":
                    options.ForceUpdate = true;
                    break;
                case "--skip-notes":
                case "-s":
                    options.SkipReleaseNotes = true;
                    break;
                case "--quiet":
                case "-q":
                    options.QuietMode = true;
                    break;
                case "--path":
                case "-p":
                    if (i + 1 < args.Length)
                    {
                        options.CustomPath = args[i + 1];
                        i++; // Skip the next argument as it's the path value
                    }
                    break;
            }
        }
        
        return options;
    }

    private void ShowHelp()
    {
        Console.WriteLine("OpenCode Updater - Downloads and installs the latest version of OpenCode");
        Console.WriteLine();
        Console.WriteLine("Usage: upgrade-opencode [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -f, --force         Force update regardless of current version");
        Console.WriteLine("  -s, --skip-notes    Skip displaying release notes");
        Console.WriteLine("  -q, --quiet         Minimal console output");
        Console.WriteLine("  -p, --path <path>   Custom installation path");
        Console.WriteLine("  --version           Show version information");
        Console.WriteLine("  -h, --help          Show this help message");
    }

    private void ShowVersion()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version?.ToString() ?? "1.0.0";
        Console.WriteLine($"upgrade-opencode version {version}");
    }
}