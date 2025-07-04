using OpenCodeUpdater.Helpers;
using OpenCodeUpdater.Services;

using System.Text.Json;
using System.Text.Json.Serialization;

using upgrade_opencode.Services;

using HttpClient httpClient = new()
{
    Timeout = TimeSpan.FromMinutes(5)
};

var validationHelpers = new ValidationHelpers();
var pathHelpers = new PathHelpers();

IConsoleOutputService consoleOutput;
try
{
    consoleOutput = new SpectreConsoleOutputService();
}
catch
{
    consoleOutput = new BasicConsoleOutputService();
}

var versionService = new OpenCodeVersionService();
var githubService = new GitHubApiService(httpClient, validationHelpers, consoleOutput);
var downloadService = new FileDownloadService(httpClient, consoleOutput);
var extractionService = new ArchiveExtractionService(pathHelpers, consoleOutput);
var platformService = new PlatformDetectionService();
var releaseNotesService = new ReleaseNotesService(githubService, versionService);

var updater = new OpenCodeUpdater.OpenCodeUpdater(
    versionService,
    githubService,
    downloadService,
    extractionService,
    platformService,
    consoleOutput,
    releaseNotesService
);

var commandLineService = new CommandLineParsingService();

var exitCode = await commandLineService.ParseAndExecuteAsync(args, updater);
Environment.Exit(exitCode);

[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(JsonDocument))]
[JsonSourceGenerationOptions(WriteIndented = false, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class GitHubApiJsonContext : JsonSerializerContext
{
}