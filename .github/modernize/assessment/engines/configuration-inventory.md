# Configuration & Externalized Settings Inventory

The solution has a minimal configuration surface: four application settings spread across two `Web.config`/`App.config` XML files, two MSBuild build configurations (Debug/Release), and no runtime profiles, feature flags, or external secret stores.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|--------|------|--------------|-------|
| `Web.config` | XML app settings | `NYCJobsWeb/Web.config` | Primary runtime configuration for the ASP.NET MVC web app; contains Azure AI Search endpoint/API key and Bing API key |
| `Web.Debug.config` | XML transform | `NYCJobsWeb/Web.Debug.config` | No active transformations; contains only commented-out examples |
| `Web.Release.config` | XML transform | `NYCJobsWeb/Web.Release.config` | Removes `debug="true"` compilation attribute on Release publish; no other overrides |
| `App.config` | XML app settings | `DataLoader/DataLoader/App.config` | Runtime configuration for the DataLoader console app; contains Azure AI Search service name and API key |
| No external config server | — | — | No Spring Cloud Config, Azure App Configuration, AWS AppConfig, or Consul KV is used |
| No secret store | — | — | No Azure Key Vault, HashiCorp Vault, or AWS Secrets Manager integration |

## Build Profiles

| Profile | Activation | Purpose | Key Changes |
|---------|-----------|---------|------------|
| Debug | Default (when `$(Configuration)` is unset) in MSBuild | Development / local debugging | `debug=true` in compilation; `DebugSymbols=true`; `Optimize=false`; `DefineConstants=DEBUG;TRACE` |
| Release | Manual (`/p:Configuration=Release`) or Visual Studio Publish | Production packaging / IIS deployment | Removes `debug` attribute via `Web.Release.config` transform; `Optimize=true`; `DefineConstants=TRACE` |

Both projects (NYCJobsWeb and DataLoader) use the same two MSBuild configurations. There are no conditional package inclusions, build plugins, or Docker packaging targets in either `.csproj`.

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---------|------------------|-------------|--------------|
| (None) | N/A | `Web.config` (NYCJobsWeb) | Single configuration used in all environments |
| (None) | N/A | `App.config` (DataLoader) | Single configuration used in all environments |

Neither project uses ASP.NET Core environment-specific configuration files (`appsettings.Development.json`, `appsettings.Production.json`), `ASPNETCORE_ENVIRONMENT`, or any `@Profile`-style conditional configuration. Configuration is flat — the same `Web.config` is used for both local development and production deployment, with placeholder values that must be manually replaced before deployment.

## Properties Inventory

### NYCJobsWeb (`Web.config`)

| Property Key | Default / Current Value | Environment | Source |
|-------------|------------------------|-------------|--------|
| `BingApiKey` (first entry) | `[ENTER BING API KEY]` | All | `Web.config` `<appSettings>` |
| `SearchServiceName` | `azs-playground` | All | `Web.config` `<appSettings>` |
| `SearchServiceApiKey` | `<api-key>` | All | `Web.config` `<appSettings>` |
| `BingApiKey` (second entry — duplicate) | `""` (empty string) | All | `Web.config` `<appSettings>` — duplicate key; second value wins |
| `webpages:Version` | `3.0.0.0` | All | `Web.config` `<appSettings>` |
| `webpages:Enabled` | `false` | All | `Web.config` `<appSettings>` |
| `ClientValidationEnabled` | `true` | All | `Web.config` `<appSettings>` |
| `UnobtrusiveJavaScriptEnabled` | `true` | All | `Web.config` `<appSettings>` |
| `system.web/compilation/@debug` | `true` | Debug only (removed in Release transform) | `Web.config` + `Web.Release.config` transform |
| `system.web/httpRuntime/@targetFramework` | `4.7.2` | All | `Web.config` |

> Note: `Web.config` contains `BingApiKey` defined twice — first with placeholder `[ENTER BING API KEY]`, second with an empty string. In ASP.NET, the second definition overrides the first, so the Bing API key is effectively blank. The `JobsSearch` class reads `Searchendpoint` (not `SearchServiceName`) from `ConfigurationManager.AppSettings` — this key is missing from `Web.config` (only `SearchServiceName` is present), which would cause a null endpoint URL at startup.

### DataLoader (`App.config`)

| Property Key | Default / Current Value | Environment | Source |
|-------------|------------------------|-------------|--------|
| `TargetSearchServiceName` | `[TARGET SEARCH SERVICE - Excluding search.windows.net]` | All | `App.config` `<appSettings>` |
| `TargetSearchServiceApiKey` | `[TARGET SEARCH SERVICE API KEY]` | All | `App.config` `<appSettings>` |

Both values are placeholder strings that must be replaced before the DataLoader can run.

## Startup Parameters & Resource Requirements

| Service | Runtime Options | Memory/CPU | Instance Count | Notes |
|---------|----------------|-----------|---------------|-------|
| NYCJobsWeb | IIS/IIS Express hosted; no custom JVM/CLR args | Not specified | 1 (single IIS application) | `UseIISExpress=true` in `.csproj`; no Docker or Kubernetes config present |
| DataLoader | .NET Framework 4.5 CLR; no custom args | Not specified | 1 (manual run) | Console app; run on demand, not as a persistent service |

No Dockerfile, Docker Compose file, Kubernetes manifests, or cloud deployment configuration files (ARM templates, Bicep, Terraform) exist in the repository.

## Startup Dependency Chain

| Startup Step | Dependency | Wait Mechanism | Notes |
|-------------|-----------|---------------|-------|
| NYCJobsWeb starts → initializes `JobsSearch` static constructor | Azure AI Search service must be reachable | None (fire-and-forget; exception is swallowed and stored in `errorMessage`) | If the Search endpoint is unreachable at startup, the app starts but all search requests return null results |
| DataLoader starts → calls Azure Search REST API | Azure AI Search service must be reachable | None | Program exits with error message if API calls fail |

There are no readiness probes, health check endpoints, `dockerize` wait-for-TCP patterns, or Kubernetes `depends_on` mechanisms.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Stored In | Value in Repo |
|-----------------|------|----------|--------------|
| `SearchServiceApiKey` (NYCJobsWeb) | Azure AI Search Query API Key | `Web.config` `<appSettings>` | `<api-key>` (placeholder — **must be replaced before deployment**) |
| `BingApiKey` (NYCJobsWeb) | Bing Maps API Key | `Web.config` `<appSettings>` | `""` (empty — **Bing geocoding non-functional**) |
| `Searchendpoint` (NYCJobsWeb) | Azure AI Search endpoint URL | `Web.config` `<appSettings>` | Missing (key `SearchServiceName` present instead — **misconfiguration**) |
| `TargetSearchServiceApiKey` (DataLoader) | Azure AI Search Admin API Key | `App.config` `<appSettings>` | `[TARGET SEARCH SERVICE API KEY]` (placeholder) |

No actual secret values are committed to the repository. However, the secrets workflow is entirely manual — there is no automated secrets provisioning, rotation, or injection mechanism.

### Secrets Provisioning Workflow

Secrets are stored as plain-text placeholder values in committed XML configuration files (`Web.config`, `App.config`). The intended workflow requires a developer to manually edit these files before deployment:

1. **NYCJobsWeb**: Edit `Web.config`, replace `<api-key>` in `SearchServiceApiKey`, add the `Searchendpoint` key with the full Azure AI Search endpoint URL (e.g., `https://{service-name}.search.windows.net`), and optionally supply a `BingApiKey`.
2. **DataLoader**: Edit `App.config`, replace both placeholder values with the actual Azure AI Search service name and Admin API key.

There is no managed identity, service principal, Azure Key Vault reference, environment variable injection, or CI/CD secrets pipeline. The configuration files are not excluded from version control (`.gitignore` does not list `Web.config` or `App.config`). Actual secret values would be committed to the repository if a developer replaces the placeholders without using a secrets management system.

## Feature Flags

No feature flag framework or conditional configuration is present in the solution. There are no `@ConditionalOnProperty` annotations (Java), `IFeatureManager` (.NET), LaunchDarkly, Unleash, or custom toggle mechanisms. All functionality is always enabled.

## Framework & Runtime Versions

| Component | Version | Source |
|-----------|---------|--------|
| .NET Framework (NYCJobsWeb) | 4.7.2 | `NYCJobsWeb.csproj` `<TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>` |
| .NET Framework (DataLoader) | 4.5 | `DataLoader.csproj` `<TargetFrameworkVersion>v4.5</TargetFrameworkVersion>` |
| ASP.NET MVC | 5.2.2 | `packages.config` `Microsoft.AspNet.Mvc` |
| ASP.NET Razor | 3.2.2 | `packages.config` `Microsoft.AspNet.Razor` |
| Azure.Search.Documents SDK | 11.1.1 | `packages.config` |
| Azure.Core | 1.4.1 | `packages.config` |
| Microsoft.Spatial | 7.5.3 | `packages.config` |
| BingGeocodingHelper | 1.1 | `packages.config` |
| Newtonsoft.Json (NYCJobsWeb) | 10.0.3 | `packages.config` |
| Newtonsoft.Json (DataLoader) | 9.0.1 | `DataLoader/packages.config` |
| Bootstrap | 3.4.1 | `packages.config` |
| jQuery | 3.1.1 | `packages.config` |
| MSBuild ToolsVersion | 12.0 | `.csproj` files |
| IIS Express | Used for local dev | `NYCJobsWeb.csproj` `<UseIISExpress>true</UseIISExpress>` |
| No Docker base image | — | No Dockerfile present |
| No CI/CD pipeline | — | No `.github/workflows/*.yml` build/deploy pipeline present |
