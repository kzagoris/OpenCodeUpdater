namespace OpenCodeUpdater.Helpers;

public class PathHelpers
{
    public string GetSafeExtractionPath(string preferredPath)
    {
        try
        {
            if (Directory.Exists(preferredPath)) return preferredPath;

            Directory.CreateDirectory(preferredPath);

            if (Directory.Exists(preferredPath))
            {
                Console.WriteLine($"Created directory: {preferredPath}");
                return preferredPath;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not create/access directory {preferredPath}: {ex.Message}");
        }

        string currentDir = AppContext.BaseDirectory;
        Console.WriteLine($"Using fallback directory: {currentDir}");
        return currentDir;
    }
}