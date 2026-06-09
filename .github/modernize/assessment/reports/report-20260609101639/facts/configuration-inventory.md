# Configuration & Externalized Settings Inventory

The repository relies on a small set of XML configuration files, classic ASP.NET build settings, and package manifests rather than a modern hierarchical configuration system. Secrets are supplied manually through app settings, and there are no separate runtime profile files beyond standard debug and release transforms.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| Web.config | Runtime application settings | `NYCJobsWeb/Web.config` | Holds Azure Search and Bing-related app settings plus ASP.NET runtime configuration |
| Web.Debug.config | Build transform | `NYCJobsWeb/Web.Debug.config` | Debug transform scaffold for the MVC site |
| Web.Release.config | Build transform | `NYCJobsWeb/Web.Release.config` | Release transform scaffold for the MVC site |
| Views web.config | MVC view configuration | `NYCJobsWeb/Views/web.config` | Configures Razor view compilation behavior |
| App.config | Runtime application settings | `DataLoader/DataLoader/App.config` | Stores Azure Search target service name and API key for the loader utility |
| packages.config files | Dependency manifests | `NYCJobsWeb/packages.config`, `DataLoader/DataLoader/packages.config` | External package versions are managed per project |
| Project and solution files | Build configuration | `NYCJobsWeb.sln`, `NYCJobsWeb/NYCJobsWeb.csproj`, `DataLoader/DataLoader/DataLoader.csproj` | Define target frameworks, output paths, IIS Express settings, and package references |
| Publish profile | Deployment metadata | `NYCJobsWeb/Properties/PublishProfiles/AZjobsdemo - Web Deploy.pubxml` | Contains classic web deploy publishing metadata |

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | Manual build configuration | Builds local development binaries with symbols and disabled optimizations | Uses the same package set; enables `DEBUG;TRACE` symbols |
| Release | Manual build configuration | Builds optimized output for deployment | Uses the same package set with `TRACE` only |

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Default | Standard ASP.NET application startup | `NYCJobsWeb/Web.config` | Provides the active Azure Search and Bing-related settings |
| Debug transform | Build-time web.config transform | `NYCJobsWeb/Web.Debug.config` | Placeholder transform file; no additional runtime profile file detected |
| Release transform | Build-time web.config transform | `NYCJobsWeb/Web.Release.config` | Placeholder transform file; no additional runtime profile file detected |
| Loader default | Console execution | `DataLoader/DataLoader/App.config` | Supplies the target Azure Search service settings for import operations |

## Properties Inventory

### NYCJobsWeb

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `BingApiKey` | `[MASKED or blank]` | Default | `NYCJobsWeb/Web.config` |
| `SearchServiceName` | `azs-playground` | Default | `NYCJobsWeb/Web.config` |
| `SearchServiceApiKey` | `[MASKED]` | Default | `NYCJobsWeb/Web.config` |
| `webpages:Version` | `3.0.0.0` | Default | `NYCJobsWeb/Web.config` |
| `webpages:Enabled` | `false` | Default | `NYCJobsWeb/Web.config` |
| `ClientValidationEnabled` | `true` | Default | `NYCJobsWeb/Web.config` |
| `UnobtrusiveJavaScriptEnabled` | `true` | Default | `NYCJobsWeb/Web.config` |
| `system.web compilation debug` | `true` | Default | `NYCJobsWeb/Web.config` |
| `system.web httpRuntime targetFramework` | `4.7.2` | Default | `NYCJobsWeb/Web.config` |

### DataLoader

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `TargetSearchServiceName` | `[MASKED placeholder]` | Default | `DataLoader/DataLoader/App.config` |
| `TargetSearchServiceApiKey` | `[MASKED placeholder]` | Default | `DataLoader/DataLoader/App.config` |
| `supportedRuntime sku` | `.NETFramework,Version=v4.5` | Default | `DataLoader/DataLoader/App.config` |

## Startup Parameters & Resource Requirements

| Service | JVM/Runtime Options | Memory | Instance Count |
|---|---|---|---|
| NYCJobsWeb | No explicit startup flags detected beyond ASP.NET target framework settings and IIS Express metadata | Not specified | Not specified |
| DataLoader | No explicit runtime options detected | Not specified | Runs on demand as a single console process |

## Startup Dependency Chain

1. Azure Cognitive Search service and its `nycjobs` and `zipcodes` indexes must exist before the MVC site can return successful search responses.
2. `DataLoader` can be run first to delete, recreate, and repopulate the indexes from the local schema and JSON files.
3. `NYCJobsWeb` starts by registering MVC routes in `Application_Start`; endpoint readiness then depends on the configured search endpoint and API key being valid.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| `SearchServiceApiKey` | Azure Cognitive Search API key | `NYCJobsWeb/Web.config` value masked |
| `BingApiKey` | Bing Maps or geocoding API key | `NYCJobsWeb/Web.config` value masked or blank |
| `TargetSearchServiceApiKey` | Azure Cognitive Search admin key for loader operations | `DataLoader/DataLoader/App.config` value masked |

### Secrets Provisioning Workflow

Secrets are provisioned manually through configuration files in the repository structure expected by classic .NET Framework applications. The web app requires Azure Search credentials, and the loader requires an admin key capable of deleting and recreating indexes. No Key Vault, managed identity, external secret store, or automated secret binding workflow was detected.

## Feature Flags

| Flag Name | Default | Controlled By |
|---|---|---|
| None detected | N/A | No feature flag framework or property-driven toggles were found |

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| .NET Framework for NYCJobsWeb | 4.7.2 | `NYCJobsWeb/NYCJobsWeb.csproj`, `NYCJobsWeb/Web.config` |
| ASP.NET MVC | 5.2.2 | `NYCJobsWeb/packages.config` |
| ASP.NET Razor | 3.2.2 | `NYCJobsWeb/packages.config` |
| ASP.NET WebPages | 3.2.2 | `NYCJobsWeb/packages.config` |
| Azure.Search.Documents | 11.1.1 | `NYCJobsWeb/packages.config` |
| Azure.Core | 1.4.1 | `NYCJobsWeb/packages.config` |
| Newtonsoft.Json for web app | 10.0.3 | `NYCJobsWeb/packages.config` |
| .NET Framework for DataLoader | 4.5 | `DataLoader/DataLoader/DataLoader.csproj`, `DataLoader/DataLoader/App.config` |
| Newtonsoft.Json for DataLoader | 9.0.1 | `DataLoader/DataLoader/packages.config` |
| MSBuild ToolsVersion | 12.0 | Both project files |
