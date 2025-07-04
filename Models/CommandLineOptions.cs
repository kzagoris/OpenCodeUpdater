namespace OpenCodeUpdater.Models;

public class CommandLineOptions
{
    public bool ForceUpdate { get; set; } = false;
    public bool SkipReleaseNotes { get; set; } = false;
    public bool QuietMode { get; set; } = false;
    public string? CustomPath { get; set; }
}