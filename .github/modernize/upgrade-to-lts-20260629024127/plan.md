# .NET Upgrade Plan: .NET Framework → .NET 10 LTS

## Overview

This plan upgrades the **JobSearch** solution from legacy .NET Framework to **.NET 10** (`net10.0`), the current Long-Term Support (LTS) release. Both projects use old-style `.csproj` formats and will require SDK-style project file conversion as part of the upgrade.

## Projects in Solution

| Project | Path | Current Framework | Target Framework |
|---------|------|-------------------|------------------|
| DataLoader (AzureSearchBackupRestore) | `DataLoader/DataLoader/DataLoader.csproj` | .NET Framework 4.5 | net10.0 |
| NYCJobsWeb | `NYCJobsWeb/NYCJobsWeb.csproj` | .NET Framework 4.7.2 | net10.0 |

## Upgrade Rationale

- **DataLoader** targets `.NET Framework v4.5`, which is end-of-life and below the minimum `.NET Framework 4.6.2` required for Azure SDK (`Azure.*`) compatibility.
- **NYCJobsWeb** targets `.NET Framework v4.7.2`. .NET Framework is in maintenance mode with no new feature development, and the user has explicitly requested an upgrade to the latest LTS version.
- Both projects use legacy-style project files that must be converted to SDK-style format.

## What the Upgrade Entails

1. **SDK-style project file conversion** — Both `.csproj` files will be rewritten in the modern SDK format.
2. **Target Framework Moniker (TFM) update** — `TargetFrameworkVersion` changed to `<TargetFramework>net10.0</TargetFramework>`.
3. **NuGet package updates** — All NuGet dependencies updated to versions compatible with `net10.0`; `packages.config` migrated to `PackageReference` format.
4. **API compatibility fixes** — Any .NET Framework-specific APIs replaced with modern .NET equivalents (e.g., `System.Web` MVC → ASP.NET Core for NYCJobsWeb).
5. **Build validation** — Solution must compile successfully against .NET 10.

## Source & Target Versions

- **Source**: .NET Framework 4.5 (DataLoader), .NET Framework 4.7.2 (NYCJobsWeb)
- **Target**: .NET 10 (`net10.0`)
