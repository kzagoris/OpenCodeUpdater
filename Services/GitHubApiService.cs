using System.Text.Json;
using OneOf;
using OpenCodeUpdater.Models;
using OpenCodeUpdater.Errors;
using OpenCodeUpdater.Helpers;
using upgrade_opencode.Services;

namespace OpenCodeUpdater.Services;

public class GitHubApiService(HttpClient httpClient, ValidationHelpers validationHelpers, IConsoleOutputService consoleOutput)
{
    public async Task<OneOf<JsonDocument, HttpError, ValidationError>> FetchLatestReleaseAsync(string repositoryUrl)
    {
        if (!validationHelpers.IsValidUrl(repositoryUrl))
        {
            return new ValidationError("Invalid GitHub API URL");
        }

        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "OpenCode-Updater");

        try
        {
            consoleOutput.WriteInfo("Fetching release information from GitHub...");
            string releaseJson = await httpClient.GetStringAsync(repositoryUrl);

            var doc = JsonDocument.Parse(releaseJson);
            var root = doc.RootElement;

            if (!root.TryGetProperty("tag_name", out var tagNameElement))
            {
                return new ValidationError("Invalid API response - missing tag_name property");
            }

            if (!root.TryGetProperty("assets", out var assetsElement))
            {
                return new ValidationError("Invalid API response - missing assets property");
            }

            return doc;
        }
        catch (HttpRequestException ex)
        {
            return new HttpError($"Failed to fetch release information from GitHub: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            return new HttpError("Request timed out while fetching release information", ex);
        }
        catch (JsonException ex)
        {
            return new ValidationError($"Invalid JSON response: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new HttpError($"Unexpected error fetching release info: {ex.Message}", ex);
        }
    }

    public OpenCodeRelease? ParseRelease(JsonDocument doc, string platformPattern, OpenCodeVersionService versionService)
    {
        var root = doc.RootElement;

        string? latestVersion = root.GetProperty("tag_name").GetString();
        if (string.IsNullOrWhiteSpace(latestVersion) || !versionService.IsValidVersion(latestVersion))
        {
            return null;
        }

        var assetsElement = root.GetProperty("assets");
        foreach (JsonElement asset in assetsElement.EnumerateArray())
        {
            string? name = asset.GetProperty("name").GetString();
            if (string.IsNullOrWhiteSpace(name))
                continue;

            if (name.Contains(platformPattern) && (name.EndsWith(".zip") || name.EndsWith(".tar.gz")))
            {
                string? downloadUrl = asset.GetProperty("browser_download_url").GetString();
                if (string.IsNullOrWhiteSpace(downloadUrl) || !validationHelpers.IsValidUrl(downloadUrl))
                    continue;

                if (!validationHelpers.IsValidFileName(name))
                    continue;

                return new OpenCodeRelease
                {
                    Version = latestVersion,
                    DownloadUrl = downloadUrl,
                    FileName = name
                };
            }
        }

        return null;
    }

    public async Task<OneOf<List<ReleaseNote>, HttpError, ValidationError>> FetchAllReleasesAsync()
    {
        const string allReleasesUrl = "https://api.github.com/repos/sst/opencode/releases";

        if (!validationHelpers.IsValidUrl(allReleasesUrl))
        {
            return new ValidationError("Invalid GitHub API URL");
        }

        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "OpenCode-Updater");

        try
        {
            string releasesJson = await httpClient.GetStringAsync(allReleasesUrl);
            var doc = JsonDocument.Parse(releasesJson);
            var releases = new List<ReleaseNote>();

            foreach (var releaseElement in doc.RootElement.EnumerateArray())
            {
                var tagName = releaseElement.GetProperty("tag_name").GetString();
                var name = releaseElement.GetProperty("name").GetString();
                var body = releaseElement.GetProperty("body").GetString();
                var publishedAt = releaseElement.GetProperty("published_at").GetString();
                var htmlUrl = releaseElement.GetProperty("html_url").GetString();

                if (string.IsNullOrWhiteSpace(tagName) || string.IsNullOrWhiteSpace(name) ||
                    string.IsNullOrWhiteSpace(body) || string.IsNullOrWhiteSpace(publishedAt) ||
                    string.IsNullOrWhiteSpace(htmlUrl))
                    continue;

                if (DateTime.TryParse(publishedAt, out var publishedDate))
                {
                    releases.Add(new ReleaseNote
                    {
                        Version = tagName,
                        Name = name,
                        Body = body,
                        PublishedAt = publishedDate,
                        HtmlUrl = htmlUrl
                    });
                }
            }

            return releases;
        }
        catch (HttpRequestException ex)
        {
            return new HttpError($"Failed to fetch releases from GitHub: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            return new HttpError("Request timed out while fetching releases", ex);
        }
        catch (JsonException ex)
        {
            return new ValidationError($"Invalid JSON response: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new HttpError($"Unexpected error fetching releases: {ex.Message}", ex);
        }
    }
}