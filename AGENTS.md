# Coding Guidelines for .NET

NEVER directly change/add dependencies in .csproj files. ALWAYS use the `dotnet` CLI to manage dependencies. If not specified otherwise, always use the latest stable version of a dependency.
