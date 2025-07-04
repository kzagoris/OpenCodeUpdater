# Command-Line Arguments Parsing Implementation Plan

## Overview

This document outlines the plan for implementing command-line argument parsing in the OpenCode Updater application using System.CommandLine. The implementation will follow the existing architecture and design patterns of the application while ensuring AOT compatibility.

## Current Architecture

The OpenCode Updater currently:
- Uses a modular design with separate services for different responsibilities
- Implements dependency injection through constructor parameters
- Has a top-level program structure in `Program.cs`
- Follows a service-oriented architecture where each service has a single responsibility

## Proposed Command-Line Arguments

Based on the application's functionality, the following command-line arguments are proposed:

1. **Force Update** (`--force`, `-f`): Skip version comparison and force download/installation
2. **Skip Release Notes** (`--skip-notes`, `-s`): Skip displaying release notes
3. **Quiet Mode** (`--quiet`, `-q`): Minimal console output (verbose output is the default)
4. **Installation Path** (`--path`, `-p`): Custom installation path
5. **Version** (`--version`): Display current tool version and exit
6. **Help** (`--help`, `-h`): Display help information

## Implementation Plan

### Considerations

 1. Please use the latest C#
 2. Use namespace of the files that use create

### 1. Create CommandLineParsingService

Create a new service to handle command-line parsing using System.CommandLine:

```csharp
using System.CommandLine;

public class CommandLineParsingService
{
    private readonly RootCommand _rootCommand;
    private readonly Option<bool> _forceOption;
    private readonly Option<bool> _skipNotesOption;
    private readonly Option<bool> _quietOption;
    private readonly Option<string> _pathOption;

    public CommandLineParsingService()
    {
        // Create options
        _forceOption = new Option<bool>(
            name: "--force",
            description: "Force update regardless of current version");
        _forceOption.AddAlias("-f");

        _skipNotesOption = new Option<bool>(
            name: "--skip-notes",
            description: "Skip displaying release notes");
        _skipNotesOption.AddAlias("-s");

        _quietOption = new Option<bool>(
            name: "--quiet",
            description: "Minimal console output");
        _quietOption.AddAlias("-q");

        _pathOption = new Option<string>(
            name: "--path",
            description: "Custom installation path");
        _pathOption.AddAlias("-p");

        // Create root command
        _rootCommand = new RootCommand("OpenCode Updater - Downloads and installs the latest version of OpenCode");
        
        // Add options to root command
        _rootCommand.AddOption(_forceOption);
        _rootCommand.AddOption(_skipNotesOption);
        _rootCommand.AddOption(_quietOption);
        _rootCommand.AddOption(_pathOption);
    }

    public async Task<int> ParseAndExecuteAsync(string[] args, OpenCodeUpdater updater)
    {
        _rootCommand.SetHandler(
            async (bool force, bool skipNotes, bool quiet, string? path) =>
            {
                bool result = await updater.UpdateAsync(
                    forceUpdate: force,
                    skipReleaseNotes: skipNotes,
                    quietMode: quiet,
                    customPath: path);
                
                return result ? 0 : 1;
            },
            _forceOption,
            _skipNotesOption,
            _quietOption,
            _pathOption);

        return await _rootCommand.InvokeAsync(args);
    }

    public CommandLineOptions ParseOptions(string[] args)
    {
        ParseResult parseResult = _rootCommand.Parse(args);
        
        return new CommandLineOptions
        {
            ForceUpdate = parseResult.GetValueForOption(_forceOption),
            SkipReleaseNotes = parseResult.GetValueForOption(_skipNotesOption),
            QuietMode = parseResult.GetValueForOption(_quietOption),
            CustomPath = parseResult.GetValueForOption(_pathOption)
        };
    }
}
```

### 2. Create CommandLineOptions Class

Create a class to hold the parsed command-line options:

```csharp
public class CommandLineOptions
{
    public bool ForceUpdate { get; set; }
    public bool SkipReleaseNotes { get; set; }
    public bool QuietMode { get; set; }
    public string? CustomPath { get; set; }
}
```

### 3. Modify OpenCodeUpdater Class

Update the `OpenCodeUpdater` class to accept the new parameters:

```csharp
public async Task<bool> UpdateAsync(
    bool forceUpdate = false,
    bool skipReleaseNotes = false,
    bool quietMode = false,
    string? customPath = null)
{
    // Set quiet mode on console output service
    if (_consoleOutput is IConsoleOutputService consoleService)
    {
        consoleService.QuietMode = quietMode;
    }
    
    // Get current version
    string? currentVersion = await _versionService.GetCurrentVersionAsync();
    
    // Get latest release
    var releaseResult = await _githubService.GetLatestReleaseAsync();
    if (releaseResult.IsT1)
    {
        _consoleOutput.WriteError($"Failed to get latest release: {releaseResult.AsT1.Message}");
        return false;
    }
    
    var release = releaseResult.AsT0;
    
    // Check if update is needed
    if (forceUpdate || currentVersion == null || _versionService.CompareVersions(currentVersion, release.Version) < 0)
    {
        // Display release notes if not skipped
        if (!skipReleaseNotes && currentVersion != null)
        {
            await _releaseNotesService.DisplayReleaseNotesAsync(currentVersion, release.Version);
        }
        
        // Download and install
        _consoleOutput.WriteInfo($"Found: {release.FileName}");
        _consoleOutput.WriteInfo($"Downloading from: {release.DownloadUrl}");
        
        // Use custom path if provided
        string extractPath = customPath ?? _extractionService.GetExtractionPath();
        
        // Rest of implementation...
        
        return true;
    }
    else
    {
        _consoleOutput.WriteSuccess($"OpenCode is already up to date (version {currentVersion})");
        return true;
    }
}
```

### 4. Update Program.cs

Modify `Program.cs` to use the new command-line parsing service:

```csharp
// Existing service initialization
var versionService = new OpenCodeVersionService();
var githubService = new GitHubApiService(httpClient, validationHelpers);
var downloadService = new FileDownloadService(httpClient, validationHelpers, consoleOutput);
var extractionService = new ArchiveExtractionService();
var platformService = new PlatformDetectionService();
var releaseNotesService = new ReleaseNotesService(githubService, consoleOutput);

// Create the updater
var updater = new OpenCodeUpdater(
    versionService,
    githubService,
    downloadService,
    extractionService,
    platformService,
    validationHelpers,
    consoleOutput,
    releaseNotesService
);

// Create command-line parsing service
var commandLineService = new CommandLineParsingService();

// Parse command line and execute
return await commandLineService.ParseAndExecuteAsync(args, updater);
```

### 5. Update Existing Services

#### IConsoleOutputService

Add support for quiet mode:

```csharp
public interface IConsoleOutputService
{
    bool QuietMode { get; set; }
    
    // Existing methods
    void WriteInfo(string message);
    void WriteSuccess(string message);
    void WriteError(string message);
    void WriteWarning(string message);
    // ...
}
```

#### BasicConsoleOutputService

Implement quiet mode:

```csharp
public class BasicConsoleOutputService : IConsoleOutputService
{
    public bool QuietMode { get; set; }
    
    public void WriteInfo(string message)
    {
        if (!QuietMode)
        {
            Console.WriteLine(message);
        }
    }
    
    public void WriteSuccess(string message)
    {
        // Always show success messages, even in quiet mode
        Console.WriteLine(message);
    }
    
    public void WriteError(string message)
    {
        // Always show error messages, even in quiet mode
        Console.Error.WriteLine($"ERROR: {message}");
    }
    
    public void WriteWarning(string message)
    {
        // Always show warning messages, even in quiet mode
        Console.WriteLine($"WARNING: {message}");
    }
    
    // Other methods with similar quiet mode checks
}
```

#### ArchiveExtractionService

Add support for custom installation paths:

```csharp
public class ArchiveExtractionService
{
    // Existing methods
    
    public string GetExtractionPath()
    {
        // Default implementation
        string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string binDir = Path.Combine(homeDir, "bin");
        
        return Directory.Exists(binDir) ? binDir : Directory.GetCurrentDirectory();
    }
    
    public async Task ExtractArchiveAsync(string archivePath, string extractPath)
    {
        // Implementation
    }
}
```

## Implementation Steps

1. Add the System.CommandLine package
   ```bash
   dotnet add package System.CommandLine --prerelease
   ```

2. Create the `CommandLineOptions` class to hold parsed options

3. Create the `CommandLineParsingService` class to handle command-line parsing

4. Modify the `OpenCodeUpdater` class to accept and use the new parameters

5. Update `Program.cs` to use the new command-line parsing service

6. Update existing services to support the new parameters:
   - Add `QuietMode` property to `IConsoleOutputService`
   - Implement quiet mode in console output services
   - Ensure `ArchiveExtractionService` can handle custom paths

## AOT Compatibility Considerations

System.CommandLine is designed to be AOT-friendly and works well with trimming. To ensure AOT compatibility:

1. Avoid using reflection-based approaches for command-line parsing
2. Use explicit delegates for command handlers instead of lambdas where possible
3. Ensure all types used in command-line options are properly preserved during trimming
4. Add any necessary trimming annotations if custom types are used as option arguments
5. Test the application with AOT compilation enabled to verify compatibility

## Conclusion

This implementation plan provides a structured approach to adding command-line argument parsing to the OpenCode Updater application using System.CommandLine. The implementation follows the existing architecture and design patterns of the application by creating a dedicated service for command-line parsing, maintaining separation of concerns, and ensuring AOT compatibility for optimal performance.