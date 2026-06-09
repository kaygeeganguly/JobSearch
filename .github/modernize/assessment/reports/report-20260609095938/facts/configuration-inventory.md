# Configuration & Externalized Settings Inventory

This inventory captures configuration sources, profiles, properties, and runtime settings used by the repository's web application and supporting console utility.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| Web.config | XML config | `NYCJobsWeb/Web.config` | Main app settings, binding redirects, runtime target |
| Web.Debug.config | Transform | `NYCJobsWeb/Web.Debug.config` | Debug transform scaffold |
| Web.Release.config | Transform | `NYCJobsWeb/Web.Release.config` | Release transform scaffold |
| App.config | XML config | `DataLoader/DataLoader/App.config` | Target Azure Search service settings |
| packages.config | NuGet manifest | `NYCJobsWeb/packages.config` | Package versions for web app |
| packages.config | NuGet manifest | `DataLoader/DataLoader/packages.config` | Package versions for console app |
| .csproj files | Build config | `NYCJobsWeb/*.csproj`, `DataLoader/*.csproj` | Framework targets, output paths, build configs |

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | Build configuration (`Configuration=Debug`) | Local debugging with symbols | Standard .NET Framework build targets |
| Release | Build configuration (`Configuration=Release`) | Optimized production build output | Standard .NET Framework build targets |

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Default | IIS/IIS Express app startup | `Web.config` | Search endpoint and API key settings |
| Default (DataLoader) | Console execution | `App.config` | Target search service name and API key |

## Properties Inventory

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `SearchServiceName` | `azs-playground` | Default | `NYCJobsWeb/Web.config` |
| `SearchServiceApiKey` | `<api-key>` placeholder | Default | `NYCJobsWeb/Web.config` |
| `BingApiKey` | empty/placeholder | Default | `NYCJobsWeb/Web.config` |
| `webpages:Version` | `3.0.0.0` | Default | `NYCJobsWeb/Web.config` |
| `ClientValidationEnabled` | `true` | Default | `NYCJobsWeb/Web.config` |
| `UnobtrusiveJavaScriptEnabled` | `true` | Default | `NYCJobsWeb/Web.config` |
| `TargetSearchServiceName` | placeholder | Default | `DataLoader/DataLoader/App.config` |
| `TargetSearchServiceApiKey` | placeholder | Default | `DataLoader/DataLoader/App.config` |

## Startup Parameters & Resource Requirements

| Service | JVM/Runtime Options | Memory | Instance Count |
|---|---|---|---|
| NYCJobsWeb | IIS / ASP.NET .NET Framework runtime | Not explicitly configured in repo | 1 (assumed dev instance) |
| DataLoader | .NET Framework console runtime | Not explicitly configured in repo | On-demand single process |

## Startup Dependency Chain

1. `NYCJobsWeb` starts under IIS/IIS Express and registers MVC routes.
2. `JobsSearch` static initialization reads app settings and constructs Azure Search clients.
3. Successful API responses depend on reachable Azure Search endpoint and valid API key.
4. `DataLoader` requires target service settings before deleting/creating indexes and importing documents.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| `SearchServiceApiKey` | API key | `Web.config` value should be treated as `[MASKED]` |
| `BingApiKey` | API key | `Web.config` value should be treated as `[MASKED]` |
| `TargetSearchServiceApiKey` | API key | `App.config` value should be treated as `[MASKED]` |

### Secrets Provisioning Workflow

Secrets are represented as configuration placeholders in checked-in config files. Deployment/runtime should inject real values via secure environment-specific configuration management before app start. The web app and data-loader each require search service credentials, and the web app optionally requires Bing API credentials for geospatial features.

## Feature Flags

| Flag Name | Default | Controlled By |
|---|---|---|
| None detected | N/A | N/A |

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| .NET Framework (NYCJobsWeb) | 4.7.2 | `NYCJobsWeb.csproj` |
| ASP.NET MVC | 5.2.2 | `NYCJobsWeb/packages.config` |
| .NET Framework (DataLoader) | 4.5 | `DataLoader.csproj` |
| Azure.Search.Documents | 11.1.1 | `NYCJobsWeb/packages.config` |
| Newtonsoft.Json | 10.0.3 / 9.0.1 | `packages.config` files |
