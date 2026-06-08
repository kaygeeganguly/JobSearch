# Configuration & Externalized Settings Inventory

This repository uses XML-based .NET Framework configuration with application settings externalized in `Web.config` and `App.config`, plus package manifests and solution/project build configuration.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| Web.config | Runtime app settings | `NYCJobsWeb/Web.config` | Stores search endpoint and API keys, MVC runtime settings, binding redirects |
| Web.Debug.config | Transform file | `NYCJobsWeb/Web.Debug.config` | Environment transform template for debug deployments |
| Web.Release.config | Transform file | `NYCJobsWeb/Web.Release.config` | Environment transform template for release deployments |
| Views/web.config | MVC view configuration | `NYCJobsWeb/Views/web.config` | Razor/view engine specific settings |
| App.config | Runtime app settings | `DataLoader/DataLoader/App.config` | Stores target search service credentials |
| packages.config | Dependency config | `NYCJobsWeb/packages.config`, `DataLoader/DataLoader/packages.config` | Declared NuGet package versions |
| .csproj/.sln files | Build configuration | `NYCJobsWeb.csproj`, `DataLoader.csproj`, `.sln` files | Build options and framework targets |

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | Default local build (`Configuration=Debug`) | Development build with symbols | Standard MSBuild targets |
| Release | Explicit build flag (`Configuration=Release`) | Optimized production build output | Standard MSBuild targets |
| AnyCPU | Default platform configuration | Shared build target architecture | Applies to both projects |

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Default (NYCJobsWeb) | ASP.NET app startup | `Web.config` (+ optional transform) | Search endpoint and keys, MVC runtime settings |
| Default (DataLoader) | Console app startup | `App.config` | Target search service name and API key |
| Debug transform | Build/publish transform | `Web.Debug.config` | Placeholder for debug-time config substitutions |
| Release transform | Build/publish transform | `Web.Release.config` | Placeholder for release-time config substitutions |

## Properties Inventory

### NYCJobsWeb

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| BingApiKey | Empty / placeholder | Default, transform-capable | Web.config appSettings |
| SearchServiceName | `azs-playground` | Default, transform-capable | Web.config appSettings |
| SearchServiceApiKey | `<api-key>` placeholder | Default, transform-capable | Web.config appSettings |
| webpages:Version | `3.0.0.0` | Default | Web.config appSettings |
| webpages:Enabled | `false` | Default | Web.config appSettings |
| ClientValidationEnabled | `true` | Default | Web.config appSettings |
| UnobtrusiveJavaScriptEnabled | `true` | Default | Web.config appSettings |

### DataLoader

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| TargetSearchServiceName | Placeholder text | Default | App.config appSettings |
| TargetSearchServiceApiKey | Placeholder text | Default | App.config appSettings |

## Startup Parameters & Resource Requirements

| Service | JVM/Runtime Options | Memory | Instance Count |
|---|---|---|---|
| NYCJobsWeb | .NET Framework ASP.NET app startup (no explicit CLI runtime flags) | Not specified in repo | Not specified in repo |
| DataLoader | .NET Framework console startup (no explicit CLI runtime flags) | Not specified in repo | Manual, on-demand process |

## Startup Dependency Chain

1. `NYCJobsWeb` starts and reads `Web.config` settings.
2. `JobsSearch` static constructor initializes Azure Search clients using configured endpoint and API key.
3. Search and lookup operations are available once external Azure AI Search connectivity succeeds.
4. `DataLoader` startup similarly depends on valid app settings and Azure AI Search availability before import steps run.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| `SearchServiceApiKey` | Azure API key | Web.config appSettings (`[MASKED]`) |
| `BingApiKey` | External API key | Web.config appSettings (`[MASKED]`) |
| `TargetSearchServiceApiKey` | Azure API key | App.config appSettings (`[MASKED]`) |

### Secrets Provisioning Workflow

Secrets are expected to be manually provisioned into config files before running the applications. Both NYCJobsWeb and DataLoader read API keys at runtime from their local config sources and pass them directly to Azure AI Search or Bing API clients. No managed identity, key vault integration, or automated secret rotation workflow was found in-repo.

## Feature Flags

| Flag Name | Default | Controlled By |
|---|---|---|
| No dedicated feature flag framework detected | N/A | N/A |

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| .NET Framework (NYCJobsWeb) | v4.7.2 | `NYCJobsWeb/NYCJobsWeb.csproj` |
| .NET Framework (DataLoader) | v4.5 | `DataLoader/DataLoader/DataLoader.csproj` |
| ASP.NET MVC | 5.2.2 | `NYCJobsWeb/packages.config` |
| Azure.Search.Documents | 11.1.1 | `NYCJobsWeb/packages.config` |
| Newtonsoft.Json (web) | 10.0.3 | `NYCJobsWeb/packages.config` |
| Newtonsoft.Json (loader) | 9.0.1 | `DataLoader/DataLoader/packages.config` |
| MSBuild project format | Legacy non-SDK style | `.csproj` files |
