namespace OpenCodeUpdater.Models;

public class ReleaseNote
{
    public required string Version { get; set; }
    public required string Name { get; set; }
    public required string Body { get; set; }
    public required DateTime PublishedAt { get; set; }
    public required string HtmlUrl { get; set; }
}