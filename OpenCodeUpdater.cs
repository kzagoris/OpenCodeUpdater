using OpenCodeUpdater.Errors;
using OpenCodeUpdater.Services;

using System.Text.Json;

using upgrade_opencode.Services;

namespace OpenCodeUpdater;

public class OpenCodeUpdater(
    OpenCodeVersionService versionService,
    GitHubApiService githubService,
    FileDownloadService downloadService,
    ArchiveExtractionService extractionService,
    PlatformDetectionService platformService,
    IConsoleOutputService consoleOutput,
    ReleaseNotesService releaseNotesService)
{

    public async Task<bool> UpdateAsync(
        bool forceUpdate = false,
        bool skipReleaseNotes = false,
        bool quietMode = false,
        string? customPath = null)
    {
        consoleOutput.QuietMode = quietMode;

        var currentVersionResult = await versionService.GetCurrentVersionAsync();
        string? currentVersion = currentVersionResult.Match(
            version => (string?)version,
            error =>
            {
                consoleOutput.WriteWarning($"Warning: {error.Message}");
                return (string?)null;
            }
        );

        if (currentVersion != null)
        {
            consoleOutput.WriteVersionInfo("Current OpenCode version", currentVersion);
        }
        else
        {
            consoleOutput.WriteWarning("OpenCode is not installed or not in PATH");
        }

        consoleOutput.WriteInfo("Fetching latest OpenCode release...");

        const string apiUrl = "https://api.github.com/repos/sst/opencode/releases/latest";
        var releaseResult = await githubService.FetchLatestReleaseAsync(apiUrl);

        return await releaseResult.Match(
            async doc => await ProcessReleaseAsync(doc, currentVersion, forceUpdate, skipReleaseNotes, customPath),
            error => Task.FromResult(HandleError(error)),
            error => Task.FromResult(HandleError(error))
        );
    }

    private async Task<bool> ProcessReleaseAsync(JsonDocument doc, string? currentVersion, bool forceUpdate, bool skipReleaseNotes, string? customPath)
    {
        string platformPattern = platformService.GetPlatformPattern();
        consoleOutput.WriteInfo($"Looking for platform: {platformPattern}");

        var release = githubService.ParseRelease(doc, platformPattern, versionService);
        if (release == null)
        {
            consoleOutput.WriteError($"Release for platform '{platformPattern}' not found!");
            return false;
        }

        consoleOutput.WriteVersionInfo("Latest OpenCode version", release.Version);

        if (!forceUpdate && currentVersion != null && versionService.CompareVersions(currentVersion, release.Version) >= 0)
        {
            consoleOutput.WriteSuccess("OpenCode is already up to date!");
            return true;
        }

        if (!skipReleaseNotes)
        {
            await DisplayReleaseNotesAsync(currentVersion, release.Version);
        }

        consoleOutput.WriteInfo($"Found: {release.FileName}");
        consoleOutput.WriteInfo($"Downloading from: {release.DownloadUrl}");

        var downloadResult = await downloadService.DownloadFileWithProgressAsync(release.DownloadUrl);
        return await downloadResult.Match(
            async fileData => await ProcessDownloadedFileAsync(fileData, release.FileName, customPath),
            error => Task.FromResult(HandleError(error))
        );
    }

    private async Task<bool> ProcessDownloadedFileAsync(byte[] fileData, string fileName, string? customPath = null)
    {
        consoleOutput.WriteSuccess($"Download completed ({fileData.Length:N0} bytes)");

        string filePath = Path.Combine(extractionService.GetExtractionPath(customPath), fileName);

        var saveResult = await downloadService.SaveFileAsync(filePath, fileData);
        return await saveResult.Match(
            async success => await ProcessSavedFileAsync(filePath, customPath),
            error => Task.FromResult(HandleError(error))
        );
    }

    private Task<bool> ProcessSavedFileAsync(string filePath, string? customPath = null)
    {
        string extractPath = extractionService.GetExtractionPath(customPath);

        consoleOutput.WriteInfo("Extracting archive...");
        var extractResult = extractionService.ExtractZipSafely(filePath, extractPath);

        var success = extractResult.Match(
            success =>
            {
                consoleOutput.WriteSuccess($"Extracted to: {extractPath}");
                return true;
            },
            error => HandleError(error)
        );

        extractionService.CleanupFile(filePath);

        if (success)
        {
            consoleOutput.WriteSuccess("Download and extraction completed successfully!");
        }

        return Task.FromResult(success);
    }

    private async Task DisplayReleaseNotesAsync(string? currentVersion, string latestVersion)
    {
        if (string.IsNullOrWhiteSpace(currentVersion))
        {
            return;
        }

        var releaseNotesResult = await releaseNotesService.GetReleaseNotesBetweenVersionsAsync(currentVersion, latestVersion);

        releaseNotesResult.Match(
            releaseNotes =>
            {
                if (releaseNotes.Count > 0)
                {
                    consoleOutput.WriteReleaseNotes(releaseNotes);
                }
                return true;
            },
            httpError =>
            {
                consoleOutput.WriteWarning($"Could not fetch release notes: {httpError.Message}");
                return false;
            },
            validationError =>
            {
                consoleOutput.WriteWarning($"Could not fetch release notes: {validationError.Message}");
                return false;
            }
        );
    }

    private bool HandleError(object error)
    {
        string message = error switch
        {
            HttpError httpError => $"HTTP Error: {httpError.Message}",
            FileError fileError => $"File Error: {fileError.Message}",
            ValidationError validationError => $"Validation Error: {validationError.Message}",
            GeneralError generalError => $"Error: {generalError.Message}",
            _ => $"Unknown Error: {error}"
        };

        consoleOutput.WriteError(message);
        return false;
    }
}