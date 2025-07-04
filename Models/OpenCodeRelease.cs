namespace OpenCodeUpdater.Models;

public class OpenCodeRelease
{
    public required string Version { get; set; }
    public required string DownloadUrl { get; set; }
    public required string FileName { get; set; }
    public string? ReleaseNotes { get; set; }
}