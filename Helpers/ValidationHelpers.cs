namespace OpenCodeUpdater.Helpers;

public class ValidationHelpers
{
    public bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        try
        {
            var uri = new Uri(url);
            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }
        catch
        {
            return false;
        }
    }

    public bool IsValidFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        char[] invalidChars = Path.GetInvalidFileNameChars();
        if (fileName.IndexOfAny(invalidChars) >= 0)
            return false;

        if (fileName.Contains("..") || fileName.Contains('/') || fileName.Contains('\\'))
            return false;

        return fileName.EndsWith(".zip") || fileName.EndsWith(".tar.gz");
    }
}