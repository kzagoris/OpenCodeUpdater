using System.Diagnostics;
using System.Text.RegularExpressions;
using OneOf;
using OpenCodeUpdater.Models;
using OpenCodeUpdater.Errors;

namespace OpenCodeUpdater.Services;

public partial class OpenCodeVersionService
{
    public async Task<OneOf<string, GeneralError>> GetCurrentVersionAsync()
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = "opencode",
            Arguments = "-v",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process process = new() { StartInfo = startInfo };

        try
        {
            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            string output = await outputTask;
            string error = await errorTask;

            if (process.ExitCode == 0)
            {
                Match match = VersionRegex().Match(output);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }

                match = SimpleVersionRegex().Match(output);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }

                return new GeneralError("Could not parse version from output");
            }
            else if (!string.IsNullOrWhiteSpace(error))
            {
                return new GeneralError($"OpenCode version check failed: {error.Trim()}");
            }
            else
            {
                return new GeneralError("OpenCode version check failed with no error message");
            }
        }
        catch (Exception ex)
        {
            return new GeneralError($"Failed to check OpenCode version: {ex.Message}", ex);
        }
    }

    public int CompareVersions(string current, string latest)
    {
        current = current.TrimStart('v');
        latest = latest.TrimStart('v');

        try
        {
            if (TryParseSemanticVersion(current, out var currentSemVer) &&
                TryParseSemanticVersion(latest, out var latestSemVer))
            {
                return currentSemVer.CompareTo(latestSemVer);
            }

            Version currentVer = new(current);
            Version latestVer = new(latest);
            return currentVer.CompareTo(latestVer);
        }
        catch
        {
            return string.Compare(current, latest, StringComparison.OrdinalIgnoreCase);
        }
    }

    public bool IsValidVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return false;

        string cleanVersion = version.TrimStart('v');
        return SemanticVersionRegex().IsMatch(cleanVersion) ||
               SimpleVersionRegex().IsMatch(cleanVersion);
    }

    private bool TryParseSemanticVersion(string version, out SemanticVersion semVer)
    {
        semVer = default;

        var match = SemanticVersionRegex().Match(version);
        if (!match.Success)
            return false;

        if (!int.TryParse(match.Groups[1].Value, out int major) ||
            !int.TryParse(match.Groups[2].Value, out int minor) ||
            !int.TryParse(match.Groups[3].Value, out int patch))
            return false;

        semVer = new SemanticVersion(major, minor, patch, match.Groups[4].Value);
        return true;
    }

    [GeneratedRegex(@"version\s+([\d\.]+)", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex VersionRegex();

    [GeneratedRegex(@"([\d]+\.[\d]+\.[\d]+)")]
    private static partial Regex SimpleVersionRegex();

    [GeneratedRegex(@"^(\d+)\.(\d+)\.(\d+)(?:-(.+))?$")]
    private static partial Regex SemanticVersionRegex();
}