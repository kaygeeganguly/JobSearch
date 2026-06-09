# .NET Upgrade Plan: JobSearch Solution

## Overview

Upgrade the **JobSearch** solution from legacy .NET Framework versions to **.NET 10.0** (latest LTS).

## Current State

| Project | Type | Current Framework |
|---------|------|-------------------|
| NYCJobsWeb | ASP.NET MVC Web Application | .NET Framework 4.7.2 |
| DataLoader (AzureSearchBackupRestore) | Console Application | .NET Framework 4.5 |

## Target State

| Project | Target Framework |
|---------|-----------------|
| NYCJobsWeb | net10.0 |
| DataLoader (AzureSearchBackupRestore) | net10.0 |

## Upgrade Rationale

- **.NET Framework 4.5** reached end of support on January 12, 2016 and is EOL.
- **.NET Framework 4.7.2** mainstream support ended April 25, 2023 and is EOL.
- The user explicitly requested an upgrade to the latest LTS version (.NET 10).
- Both projects use legacy non-SDK-style `.csproj` format and require SDK-style conversion.

## Upgrade Scope

1. **SDK-style project file conversion** — both `.csproj` files must be converted from the legacy MSBuild format to the modern SDK-style format.
2. **Target Framework Moniker (TFM) update** — change `TargetFrameworkVersion` to `<TargetFramework>net10.0</TargetFramework>`.
3. **NuGet package updates** — update all NuGet dependencies to versions compatible with `net10.0`. Remove `packages.config` in favour of `PackageReference`.
4. **ASP.NET MVC → ASP.NET Core migration** — `NYCJobsWeb` uses `System.Web`-based ASP.NET MVC 5, which is not supported on .NET Core / .NET 5+. The project must be migrated to ASP.NET Core MVC.
5. **API compatibility fixes** — replace or update any APIs removed or changed between .NET Framework and .NET 10.
6. **Build validation** — confirm solution builds and all existing tests pass on .NET 10.

## Projects in Solution

- `NYCJobsWeb\NYCJobsWeb.csproj` — ASP.NET MVC 5 web application (Azure Search job listing front-end)
- `DataLoader\DataLoader\DataLoader.csproj` — Console application for loading data into Azure Search
