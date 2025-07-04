using System.IO.Compression;
using OneOf;
using OpenCodeUpdater.Errors;
using OpenCodeUpdater.Helpers;
using upgrade_opencode.Services;

namespace OpenCodeUpdater.Services;

public class ArchiveExtractionService(PathHelpers pathHelpers, IConsoleOutputService consoleOutput)
{

    public OneOf<Success, FileError> ExtractZipSafely(string zipPath, string extractPath)
    {
        try
        {
            using var archive = ZipFile.OpenRead(zipPath);

            foreach (var entry in archive.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                string entryName = entry.FullName.Replace('\\', '/');
                if (entryName.Contains("..") || Path.IsPathRooted(entryName))
                {
                    consoleOutput.WriteWarning($"Skipping potentially unsafe path: {entryName}");
                    continue;
                }

                string destinationPath = Path.Combine(extractPath, entryName);
                string normalizedDestination = Path.GetFullPath(destinationPath);
                string normalizedExtractPath = Path.GetFullPath(extractPath);

                if (!normalizedDestination.StartsWith(normalizedExtractPath) &&
                    !normalizedDestination.Equals(normalizedExtractPath))
                {
                    consoleOutput.WriteWarning($"Skipping path outside extraction directory: {entryName}");
                    continue;
                }

                string? directoryPath = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                entry.ExtractToFile(destinationPath, overwrite: true);
            }

            return new Success();
        }
        catch (Exception ex)
        {
            return new FileError($"Failed to extract archive: {ex.Message}", ex);
        }
    }

    public string GetExtractionPath(string? customPath)
    {
        if (!string.IsNullOrWhiteSpace(customPath))
        {
            return pathHelpers.GetSafeExtractionPath(customPath);
        }
        return AppContext.BaseDirectory;
    }

    public void CleanupFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                consoleOutput.WriteInfo("Downloaded file cleaned up successfully.");
            }
            catch (Exception ex)
            {
                consoleOutput.WriteWarning($"Could not delete temporary file {filePath}: {ex.Message}");
            }
        }
    }
}