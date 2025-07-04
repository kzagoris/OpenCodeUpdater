namespace OpenCodeUpdater.Models;

public class InstallationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ExtractedToPath { get; set; }

    public static InstallationResult CreateSuccess(string extractedToPath)
    {
        return new InstallationResult
        {
            Success = true,
            ExtractedToPath = extractedToPath
        };
    }

    public static InstallationResult CreateFailure(string errorMessage)
    {
        return new InstallationResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}