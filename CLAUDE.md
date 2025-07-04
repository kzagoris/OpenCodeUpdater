# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET 9.0 console application that downloads and installs the latest OpenCode release from GitHub.

### Core Functionality
- **Command-line argument parsing** with support for various operational modes
- Checks the current installed OpenCode version (if available)
- Fetches the latest release from the sst/opencode GitHub repository
- Compares versions and skips download if already up to date
- **Displays release notes** for all versions between current and latest when update is available
- Downloads the appropriate platform version (Windows, macOS, Linux)
- Extracts it to the appropriate directory
- Cleans up temporary files

## Development Commands

### Build Commands
```bash
# Build the project
dotnet build

# Build for release
dotnet build --configuration Release

# Publish as AOT (Ahead-of-Time compiled)
dotnet publish --configuration Release
```

### Run Commands
```bash
# Run the application
dotnet run

# Run with arguments
dotnet run -- [arguments]

# Common usage examples:
dotnet run -- --help                    # Show help information
dotnet run -- --version                 # Show version information
dotnet run -- --force                   # Force update regardless of current version
dotnet run -- --skip-notes              # Skip displaying release notes
dotnet run -- --quiet                   # Minimal console output
dotnet run -- --path ~/custom-bin       # Install to custom directory
dotnet run -- --force --quiet           # Force update with minimal output
```

### Command-Line Arguments
- `--force` / `-f`: Force update regardless of current version
- `--skip-notes` / `-s`: Skip displaying release notes during update
- `--quiet` / `-q`: Enable quiet mode with minimal console output
- `--path <path>` / `-p <path>`: Specify custom installation directory
- `--version`: Display version information and exit
- `--help` / `-h`: Show help message and exit

## Architecture & Design

### Application Structure
- **Modular design**: Code is organized into separate services, models, and helpers
- **Dependency injection**: Services are injected through constructor parameters
- **Separation of concerns**: Each class has a single responsibility
- **Top-level program**: Uses C# 9+ top-level statements in minimal `Program.cs` with command-line parsing
- **Parameter passing**: Optional parameters flow through services for operational modes
- **AOT compilation**: Configured for Ahead-of-Time compilation with trimming for smaller executables

### Project Structure
```
├── Program.cs                    # Entry point with dependency setup
├── OpenCodeUpdater.cs           # Main orchestrator class
├── Services/                    # Business logic services
│   ├── OpenCodeVersionService.cs    # Version checking and comparison
│   ├── GitHubApiService.cs          # GitHub API interactions
│   ├── ReleaseNotesService.cs       # Release notes fetching and filtering
│   ├── FileDownloadService.cs       # File downloads with progress
│   ├── ArchiveExtractionService.cs  # ZIP extraction and cleanup
│   ├── PlatformDetectionService.cs  # Platform detection
│   ├── CommandLineParsingService.cs # Command-line argument parsing
│   ├── IConsoleOutputService.cs     # Console output interface
│   ├── SpectreConsoleOutputService.cs # Rich console output with Spectre.Console
│   └── BasicConsoleOutputService.cs # Fallback console output
├── Models/                      # Data models
│   ├── OpenCodeRelease.cs           # GitHub release representation
│   ├── ReleaseNote.cs               # Individual release information
│   ├── SemanticVersion.cs           # Semantic version handling
│   ├── CommandLineOptions.cs        # Command-line options model
│   └── InstallationResult.cs        # Installation outcome
├── Helpers/                     # Utility classes
│   ├── ValidationHelpers.cs         # URL and file validation
│   └── PathHelpers.cs               # Safe path operations
└── Errors/                      # Error handling
    └── ErrorTypes.cs                # OneOf error types
```

### Key Technologies
- **HTTP client**: Uses `HttpClient` to interact with GitHub API
- **JSON parsing**: Uses `System.Text.Json` for parsing GitHub API responses
- **Console UI**: Uses `Spectre.Console` for rich terminal output with fallback to basic console
- **File operations**: Uses `System.IO.Compression` for ZIP file extraction
- **Process execution**: Uses `System.Diagnostics.Process` to check current OpenCode version
- **Source generators**: Uses regex source generators for efficient version parsing
- **Error handling**: Uses `OneOf` library for functional error handling
- **Dependency management**: Manual dependency injection for simplicity

### Code Quality Features
- Nullable reference types enabled
- Implicit usings for cleaner code
- Proper disposal patterns for HTTP client and processes
- Comprehensive error handling with OneOf pattern
- Separation of concerns with dedicated service classes
- Testable architecture with injected dependencies
- Comprehensive input validation and security measures

## Implementation Details

### GitHub API Integration
- `GitHubApiService` handles all GitHub API interactions
- Targets the GitHub API endpoint for the sst/opencode repository
- Fetches latest release information and asset URLs
- **Fetches all releases** for release notes display between versions
- Supports multiple platforms (Windows, macOS, Linux) with appropriate asset detection

### Version Management
- `OpenCodeVersionService` handles version operations
- Checks current OpenCode version using `opencode -v` command
- Compares semantic versions to determine if update is needed
- Implements regex source generators for efficient version parsing

### Release Notes Display
- `ReleaseNotesService` manages release notes functionality
- Fetches all releases from GitHub API and filters between current and latest versions
- Displays formatted release notes with version, date, and content when update is available
- Integrates with console output services for rich formatting
- Truncates long release notes with links to full versions for better UX
- Only shows release notes when an update is needed (current version < latest version)

### Platform Detection
- `PlatformDetectionService` automatically detects current platform
- Supports Windows (x86/x64), macOS (x64/ARM64), and Linux (x64/ARM64)
- Provides appropriate asset patterns for each platform

### Console Output
- `IConsoleOutputService` interface provides abstraction for console output
- `SpectreConsoleOutputService` delivers rich, colorful terminal output with progress bars
- `BasicConsoleOutputService` provides fallback for environments without Spectre.Console
- Supports version information, release notes, progress reporting, and status messages
- Automatically detects and falls back to basic console if Spectre.Console fails

### File Operations
- `FileDownloadService` handles downloads with progress reporting
- `ArchiveExtractionService` safely extracts ZIP files with path traversal protection
- Downloads and extracts to `~/bin` directory if it exists, otherwise to current directory
- Automatically cleans up temporary files after installation

### Command-Line Interface
- `CommandLineParsingService` handles argument parsing and application flow control
- Supports multiple operational modes: force update, quiet mode, custom paths, release notes skipping
- Uses manual parsing approach for flexibility and simplicity
- Integrates with all services through optional parameters
- Provides comprehensive help and version information

### Error Handling
- Uses OneOf pattern for functional error handling
- Specific error types: `HttpError`, `FileError`, `ValidationError`, `GeneralError`
- Comprehensive error messages and graceful degradation

## External Libraries Documentation

This project uses several external libraries. Here are links to their official documentation:

### Core Dependencies

#### OneOf (Functional Error Handling)
- **NuGet Package**: `OneOf` v3.0.271
- **Purpose**: Provides discriminated unions for functional error handling patterns
- **Documentation**: https://github.com/mcintyre321/OneOf
- **Usage in Project**: Error handling throughout services (HttpError, FileError, ValidationError, GeneralError)

#### Spectre.Console (Rich Terminal UI)
- **NuGet Package**: `Spectre.Console` v0.50.0
- **Purpose**: Rich, interactive terminal user interfaces with colors, progress bars, and markup
- **Documentation**: https://spectreconsole.net/
- **API Reference**: https://spectreconsole.net/api/
- **Usage in Project**: Progress bars, colored output, release notes formatting, version display

#### System.Text.Json (JSON Serialization)
- **NuGet Package**: `System.Text.Json` v9.0.0
- **Purpose**: High-performance JSON serialization and deserialization
- **Documentation**: https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/
- **Usage in Project**: GitHub API response parsing, source generators for AOT compatibility

#### System.CommandLine (Command-Line Interface)
- **NuGet Package**: `System.CommandLine` v2.0.0-beta5.25306.1
- **Purpose**: Command-line argument parsing and validation with modern API
- **Documentation**: https://docs.microsoft.com/en-us/dotnet/standard/commandline/
- **Usage in Project**: Command-line argument parsing for operational modes (force, quiet, custom paths)

### Framework Features

#### .NET 9.0
- **Documentation**: https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9
- **AOT Publishing**: https://docs.microsoft.com/en-us/dotnet/core/deploying/native-aot/

#### Regex Source Generators
- **Documentation**: https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-source-generators
- **Usage in Project**: Version parsing in `OpenCodeVersionService`