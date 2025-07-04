# AGENTS.md

1. Build / Lint / Test
   - Build (Debug): `dotnet build`
   - Build (Release): `dotnet build --configuration Release`
   - Publish AOT: `dotnet publish --configuration Release`
   - Lint / Format: `dotnet format` (install with `dotnet tool install -g dotnet-format`)
   - Run: `dotnet run`
   - Test all: `dotnet test`
   - Single test: `dotnet test --filter FullyQualifiedName~<TestMethodName>`

2. Code Style
   - Imports: `using` statements sorted alphabetically, system namespaces first
   - Formatting: 4-space indentation, no tabs, 120-char line length, use `dotnet format`
   - Types: Prefer explicit types; use `var` only when type is obvious
   - Naming: PascalCase for classes/methods/properties, camelCase for locals/parameters, constants in PascalCase
   - File names: Match public type names
   - Error handling: Use try/catch with specific exceptions; log and rethrow or exit non-zero
   - Nullability: Nullable reference types enabled; avoid `!` unless necessary
   - Async: Use async/await, suffix async methods with `Async`
   - Version parsing: Use regex source generators in `Program.cs`
   - Trimming: Keep `EnableTrimAnalyzer` warnings clean (treat as errors)
   - No Cursor or Copilot rules present as of this commit
   - Follow CLAUDE.md for download/update logic and architecture
