using OpenCodeUpdater.Models;

namespace upgrade_opencode.Services;

public interface IConsoleOutputService
{
    bool QuietMode { get; set; }
    void WriteInfo(string message);
    void WriteSuccess(string message);
    void WriteWarning(string message);
    void WriteError(string message);
    void WriteVersionInfo(string label, string version);
    void WriteReleaseNotes(List<ReleaseNote> releaseNotes);
    IProgressReporter CreateProgressReporter(string description, long totalBytes);
}

public interface IProgressReporter : IDisposable
{
    void UpdateProgress(long downloadedBytes, long totalBytes);
    void Complete();
}