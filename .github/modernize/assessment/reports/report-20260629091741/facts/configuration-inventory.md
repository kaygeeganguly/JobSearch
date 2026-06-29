# Configuration & Externalized Settings Inventory

The NYC Jobs solution contains three XML-based configuration files across two projects; all secrets are stored as plain text in these files with no external secret store or profile-based override mechanism in use.

## Configuration Sources

| Source | Type | Path / Location | Notes |
|--------|------|----------------|-------|
| Web.config | XML AppSettings + system.web | `NYCJobsWeb/Web.config` | Primary runtime configuration for the ASP.NET MVC 5 web application |
| Web.Debug.config | XDT Transform (Debug) | `NYCJobsWeb/Web.Debug.config` | No-op transform; contains only commented-out examples |
| Web.Release.config | XDT Transform (Release) | `NYCJobsWeb/Web.Release.config` | Removes the `debug` attribute from `<compilation>` on publish |
| App.config | XML AppSettings | `DataLoader/DataLoader/App.config` | Runtime configuration for the DataLoader console tool |
| packages.config (x2) | NuGet package manifest | `NYCJobsWeb/packages.config`, `DataLoader/DataLoader/packages.config` | Declares NuGet package versions; not a runtime config source |
| Schema_and_Data/*.schema | Azure AI Search index schema | `NYCJobsWeb/Schema_and_Data/nycjobs.schema`, `zipcodes.schema` | JSON schema definitions applied once at index creation |

No environment variables, `.env` files, Azure App Configuration, Azure Key Vault, Spring Cloud Config, Kubernetes ConfigMaps, or Docker Compose environment sections are in use.

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---------|-----------|---------|--------------------------|
| Debug | Default in Visual Studio / MSBuild `Configuration=Debug` | Development build with full debug symbols and `compilation debug="true"` | Full debug PDB output; `Web.Debug.config` transform applied (no-op) |
| Release | Manual — `MSBuild /p:Configuration=Release` or publish | Production publish build; removes debug attribute from compilation | `Web.Release.config` transform applied (removes `debug="true"`) |

No Maven/Gradle build profiles exist; the solution uses standard MSBuild configuration switching.

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---------|-----------------|-------------|---------------|
| (Single profile) | None — ASP.NET MVC 5 does not use `ASPNETCORE_ENVIRONMENT` | `Web.config` | No profile-based overrides; all environments share the same `Web.config` values |

The application uses the classic ASP.NET (non-Core) hosting model and does not support `appsettings.{Environment}.json` or `ASPNETCORE_ENVIRONMENT`-based profile switching. Environment differences (Dev vs Production) must be managed by manually editing `Web.config` or using the Debug/Release XDT transforms.

## Properties Inventory

### NYCJobsWeb (`Web.config`)

| Property Key | Default / Current Value | Profile | Source |
|-------------|------------------------|---------|--------|
| `BingApiKey` | `[ENTER BING API KEY]` (placeholder — appears twice) | All | `Web.config` appSettings |
| `SearchServiceName` | `azs-playground` | All | `Web.config` appSettings |
| `SearchServiceApiKey` | `<api-key>` (placeholder) | All | `Web.config` appSettings |
| `Searchendpoint` | *(not present — consumed via `Searchendpoint` key but not shown in config)* | All | `Web.config` appSettings — key used in `JobsSearch.cs` |
| `webpages:Version` | `3.0.0.0` | All | `Web.config` appSettings |
| `webpages:Enabled` | `false` | All | `Web.config` appSettings |
| `ClientValidationEnabled` | `true` | All | `Web.config` appSettings |
| `UnobtrusiveJavaScriptEnabled` | `true` | All | `Web.config` appSettings |
| `targetFramework` (httpRuntime) | `4.7.2` | All | `Web.config` system.web |
| `compilation debug` | `true` (Debug) / removed (Release) | Debug / Release | `Web.config` + XDT transform |

> Note: `JobsSearch.cs` reads `ConfigurationManager.AppSettings["Searchendpoint"]` but this key is absent from the committed `Web.config`. The `SearchServiceName` key is present but is not consumed by `JobsSearch.cs`. The effective endpoint is constructed from `Searchendpoint`.

### DataLoader (`App.config`)

| Property Key | Default / Current Value | Profile | Source |
|-------------|------------------------|---------|--------|
| `TargetSearchServiceName` | `[TARGET SEARCH SERVICE - Excluding search.windows.net]` (placeholder) | All | `App.config` appSettings |
| `TargetSearchServiceApiKey` | `[TARGET SEARCH SERVICE API KEY]` (placeholder) | All | `App.config` appSettings |
| `supportedRuntime` | `v4.0 / .NETFramework,Version=v4.5` | All | `App.config` startup |

## Startup Parameters & Resource Requirements

| Service | Runtime Options | Memory | Instance Count |
|---------|----------------|--------|----------------|
| NYCJobsWeb | IIS-hosted; no explicit JVM/CLR startup parameters configured in repo | Not specified | Not specified — scales with IIS worker process config |
| DataLoader | Console app; run manually from command line; no startup flags required | Not specified | 1 (run once at index setup) |

No Docker Compose files, Kubernetes manifests, or IIS/Azure App Service deployment configuration are present in the repository.

## Startup Dependency Chain

The application has a simple two-step dependency:

1. **DataLoader** (run once, offline) → creates `nycjobs` and `zipcodes` indices in Azure AI Search
2. **NYCJobsWeb** (IIS-hosted) → connects to Azure AI Search at static initialization time (`JobsSearch` static constructor)

There is no dockerize wait mechanism, no Kubernetes readiness probe, no retry-on-startup, and no health check endpoint. If Azure AI Search is unavailable at startup, `JobsSearch` will catch the exception, store the error message in the static `errorMessage` field, and return `null` from search methods — failing silently.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage |
|-----------------|------|---------|
| `SearchServiceApiKey` (`Web.config`) | Azure AI Search Query API Key | Plain text in `Web.config` (committed to source control as placeholder `<api-key>`) |
| `BingApiKey` (`Web.config`) | Bing Maps API Key | Plain text in `Web.config` (committed as placeholder `[ENTER BING API KEY]`) |
| `Searchendpoint` (`Web.config`) | Azure AI Search service endpoint URL | Plain text in `Web.config` (key referenced in code; value absent from committed file) |
| `TargetSearchServiceApiKey` (`App.config`) | Azure AI Search Admin API Key | Plain text in `App.config` (committed as placeholder `[TARGET SEARCH SERVICE API KEY]`) |
| `TargetSearchServiceName` (`App.config`) | Azure AI Search service name | Plain text in `App.config` (committed as placeholder) |

### Secrets Provisioning Workflow

No automated secrets provisioning workflow is implemented. Secrets are expected to be manually inserted into `Web.config` and `App.config` before deployment. There is no integration with Azure Key Vault, HashiCorp Vault, DPAPI encryption, Jasypt, GitHub Actions secrets injection, or any other secrets management system.

**Current workflow (manual):**
1. Developer copies `Web.config` and replaces placeholder values for `SearchServiceApiKey`, `BingApiKey`, and `Searchendpoint` with real values before deploying to IIS.
2. Developer copies `App.config` and replaces `TargetSearchServiceName` and `TargetSearchServiceApiKey` before running DataLoader.

**Risk:** The repository contains placeholder values that indicate the real secrets are expected to be substituted manually. If a developer accidentally commits real key values, they would be exposed in source history. No `.gitignore` rule prevents this. Migrating to Azure Key Vault references or environment variable overrides is strongly recommended before moving to production.

## Feature Flags

No feature flag framework is in use (no LaunchDarkly, Unleash, .NET FeatureManagement, `@ConditionalOnProperty`, or custom toggle mechanism). The `webpages:Enabled = false` setting in `Web.config` disables ASP.NET Web Pages (a framework-level switch, not a business feature flag).

| Flag Name | Default | Controlled By |
|-----------|---------|--------------|
| `webpages:Enabled` | `false` | `Web.config` appSettings |
| `ClientValidationEnabled` | `true` | `Web.config` appSettings |
| `UnobtrusiveJavaScriptEnabled` | `true` | `Web.config` appSettings |
| `compilation debug` | `true` (Debug build) | MSBuild configuration + XDT transform |

## Framework & Runtime Versions

| Component | Version | Source |
|-----------|---------|--------|
| .NET Framework (NYCJobsWeb) | 4.7.2 | `NYCJobsWeb/NYCJobsWeb.csproj` `TargetFrameworkVersion`, `Web.config` httpRuntime |
| .NET Framework (DataLoader) | 4.5 | `DataLoader/DataLoader/DataLoader.csproj` `TargetFrameworkVersion`, `App.config` |
| ASP.NET MVC | 5.2.2 | `NYCJobsWeb/packages.config` |
| ASP.NET Razor | 3.2.2 | `NYCJobsWeb/packages.config` |
| ASP.NET WebPages | 3.2.2 | `NYCJobsWeb/packages.config` |
| Azure.Search.Documents SDK | 11.1.1 | `NYCJobsWeb/packages.config` |
| Azure.Core | 1.4.1 | `NYCJobsWeb/packages.config` |
| Newtonsoft.Json (NYCJobsWeb) | 10.0.3 | `NYCJobsWeb/packages.config` |
| Newtonsoft.Json (DataLoader) | 9.0.1 | `DataLoader/DataLoader/packages.config` |
| BingGeocodingHelper | 1.1 | `NYCJobsWeb/packages.config` |
| Microsoft.Spatial | 7.5.3 | `NYCJobsWeb/packages.config` |
| Bootstrap | 3.4.1 | `NYCJobsWeb/packages.config` |
| jQuery | 3.1.1 | `NYCJobsWeb/packages.config` |
| MSBuild (ToolsVersion) | 12.0 | `DataLoader/DataLoader/DataLoader.csproj`; `NYCJobsWeb/NYCJobsWeb.csproj` |
| IIS Express / IIS | Not specified | Hosting environment (not in repo) |
