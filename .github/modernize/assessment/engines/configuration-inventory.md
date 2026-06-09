# Configuration & Externalized Settings Inventory

This repository uses a small set of .NET configuration files and app settings for Azure AI Search connectivity, build modes, and runtime behavior across a web app and a console data-loader.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| Web.config | .NET app configuration | NYCJobsWeb/Web.config | Primary runtime settings and assembly binding redirects for web app |
| Web.Debug.config | .NET transform | NYCJobsWeb/Web.Debug.config | Debug-time config transform |
| Web.Release.config | .NET transform | NYCJobsWeb/Web.Release.config | Release-time config transform |
| Views/web.config | ASP.NET MVC view config | NYCJobsWeb/Views/web.config | Razor/view engine settings |
| App.config | .NET app configuration | DataLoader/DataLoader/App.config | Loader service endpoint and API key settings |
| packages.config | NuGet dependency manifest | NYCJobsWeb/packages.config, DataLoader/DataLoader/packages.config | Package version declarations |
| .csproj build files | MSBuild project configuration | NYCJobsWeb/NYCJobsWeb.csproj, DataLoader/DataLoader/DataLoader.csproj | Target framework, references, build configuration |

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | `Configuration=Debug` | Local development builds with symbols | `DefineConstants=DEBUG;TRACE`, non-optimized output |
| Release | `Configuration=Release` | Deployment-ready optimized builds | `DefineConstants=TRACE`, optimized output |

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Default (web) | IIS/IIS Express launch | Web.config + transform chain | Search endpoint/key app settings, MVC/runtime settings |
| Default (loader) | Console app launch | App.config | Target search service name and API key |

## Properties Inventory

### NYCJobsWeb

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| BingApiKey | Empty/placeholder | Default | Web.config appSettings |
| SearchServiceName | `azs-playground` | Default | Web.config appSettings |
| SearchServiceApiKey | `[MASKED]` placeholder | Default | Web.config appSettings |
| webpages:Version | `3.0.0.0` | Default | Web.config appSettings |
| webpages:Enabled | `false` | Default | Web.config appSettings |
| ClientValidationEnabled | `true` | Default | Web.config appSettings |
| UnobtrusiveJavaScriptEnabled | `true` | Default | Web.config appSettings |

### DataLoader

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| TargetSearchServiceName | Placeholder text | Default | App.config appSettings |
| TargetSearchServiceApiKey | `[MASKED]` placeholder | Default | App.config appSettings |

## Startup Parameters & Resource Requirements

| Service | JVM/Runtime Options | Memory | Instance Count |
|---|---|---|---|
| NYCJobsWeb | .NET Framework 4.7.2 ASP.NET runtime | Not explicitly configured | Not explicitly configured |
| DataLoader | .NET Framework 4.5 console runtime | Not explicitly configured | Single process execution |

## Startup Dependency Chain

1. Azure AI Search service must be available before `NYCJobsWeb` search endpoints can return functional results.
2. `DataLoader` requires the target Azure AI Search endpoint and key before index recreation and data import.
3. No additional orchestrator-driven startup dependencies are defined in repository configuration.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| SearchServiceApiKey | API key | Web.config appSettings (`[MASKED]`) |
| TargetSearchServiceApiKey | API key | App.config appSettings (`[MASKED]`) |
| BingApiKey | API key | Web.config appSettings (`[MASKED]` / empty) |

### Secrets Provisioning Workflow

Secrets are expected to be provided manually through configuration file values before runtime. The web app consumes search and Bing keys from `Web.config`, while the loader consumes target search credentials from `App.config`. No external secret manager integration or managed-identity flow is defined in this repository.

## Feature Flags

| Flag Name | Default | Controlled By |
|---|---|---|
| webpages:Enabled | false | Web.config appSettings |
| ClientValidationEnabled | true | Web.config appSettings |
| UnobtrusiveJavaScriptEnabled | true | Web.config appSettings |

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| .NET Framework (web) | 4.7.2 | NYCJobsWeb.csproj target framework |
| .NET Framework (loader) | 4.5 | DataLoader.csproj target framework |
| ASP.NET MVC | 5.2.2 | NYCJobsWeb packages.config |
| Azure.Search.Documents | 11.1.1 | NYCJobsWeb packages.config |
| Newtonsoft.Json | 10.0.3 (web), 9.0.1 (loader) | packages.config files |
| jQuery | 3.1.1 | NYCJobsWeb packages.config |
| bootstrap | 3.4.1 | NYCJobsWeb packages.config |
