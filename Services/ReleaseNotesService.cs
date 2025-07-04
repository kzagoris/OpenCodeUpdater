using OneOf;
using OpenCodeUpdater.Models;
using OpenCodeUpdater.Services;
using OpenCodeUpdater.Errors;

namespace OpenCodeUpdater.Services;

public class ReleaseNotesService
{
    private readonly GitHubApiService _githubService;
    private readonly OpenCodeVersionService _versionService;

    public ReleaseNotesService(GitHubApiService githubService, OpenCodeVersionService versionService)
    {
        _githubService = githubService;
        _versionService = versionService;
    }

    public async Task<OneOf<List<ReleaseNote>, HttpError, ValidationError>> GetReleaseNotesBetweenVersionsAsync(string? currentVersion, string latestVersion)
    {
        if (string.IsNullOrWhiteSpace(currentVersion))
        {
            return new List<ReleaseNote>();
        }

        var allReleasesResult = await _githubService.FetchAllReleasesAsync();
        
        return allReleasesResult.Match<OneOf<List<ReleaseNote>, HttpError, ValidationError>>(
            releases => FilterReleasesBetweenVersions(releases, currentVersion, latestVersion),
            httpError => httpError,
            validationError => validationError
        );
    }

    private List<ReleaseNote> FilterReleasesBetweenVersions(List<ReleaseNote> allReleases, string currentVersion, string latestVersion)
    {
        var filteredReleases = new List<ReleaseNote>();
        
        foreach (var release in allReleases)
        {
            var compareToLatest = _versionService.CompareVersions(release.Version, latestVersion);
            var compareToCurrent = _versionService.CompareVersions(release.Version, currentVersion);
            
            if (compareToLatest <= 0 && compareToCurrent > 0)
            {
                filteredReleases.Add(release);
            }
        }
        
        filteredReleases.Sort((a, b) => _versionService.CompareVersions(b.Version, a.Version));
        
        return filteredReleases;
    }
}