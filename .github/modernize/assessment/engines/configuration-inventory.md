# Configuration & Externalized Settings Inventory

The solution uses three XML-based configuration files (`Web.config`, `Web.Debug.config`, `Web.Release.config` for NYCJobsWeb; `App.config` for DataLoader) as its only configuration sources, with no environment-variable injection, no secrets manager, and no external config server.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| Web.config | XML App Settings | `NYCJobsWeb/Web.config` | Primary runtime config for ASP.NET MVC app; holds API keys and assembly bindings |
| Web.Debug.config | XML Transform | `NYCJobsWeb/Web.Debug.config` | XDT transform applied in Debug build; no active transformations defined (stub only) |
| Web.Release.config | XML Transform | `NYCJobsWeb/Web.Release.config` | XDT transform applied in Release build; removes `debug=true` compilation attribute |
| App.config | XML App Settings | `DataLoader/DataLoader/App.config` | Runtime config for DataLoader console tool; holds target search service name and admin API key |

No `appsettings.json`, `appsettings.{Environment}.json`, `launchSettings.json`, `.env` files, Spring Cloud Config, Azure App Configuration, AWS AppConfig, Consul KV, or any external configuration repository detected.

## Build Profiles

| Profile | Activation | Purpose | Key Changes |
|---|---|---|---|
| Debug | Default in Visual Studio / `msbuild /p:Configuration=Debug` | Local development build with full debug symbols | `Web.Debug.config` XDT applied; `debug=true` left in `<compilation>`; output to `bin\` |
| Release | `msbuild /p:Configuration=Release` | Production publishing build | `Web.Release.config` XDT applied; removes `debug` attribute from `<compilation>` (disables debug compilation) |

No Maven, Gradle, npm, or Docker build profiles are present. .NET Framework projects use the standard two-configuration model (Debug/Release) with MSBuild `ToolsVersion="12.0"`.

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| (Single profile) | N/A — no profile system configured | `Web.config` / `App.config` | No per-environment overrides beyond XDT transforms |

Neither `ASPNETCORE_ENVIRONMENT` nor any profile-switching mechanism is configured. The application is .NET Framework 4.7.2 (not ASP.NET Core), so `appsettings.{Environment}.json` overrides and `IConfiguration` environment profiles are not applicable. All runtime configuration is static in `Web.config`.

## Properties Inventory

### NYCJobsWeb (`Web.config`)

| Property Key | Default / Current Value | Profile | Source |
|---|---|---|---|
| `BingApiKey` (first entry) | `[ENTER BING API KEY]` (placeholder) | All | Web.config appSettings |
| `SearchServiceName` | `azs-playground` | All | Web.config appSettings |
| `SearchServiceApiKey` | `<api-key>` (placeholder) | All | Web.config appSettings |
| `BingApiKey` (second entry) | `""` (empty string) | All | Web.config appSettings — duplicate key, second value wins |
| `Searchendpoint` | _(not present in `<appSettings>`)_ | All | Read by `JobsSearch` via `ConfigurationManager.AppSettings["Searchendpoint"]`; not set — runtime `NullReferenceException` if not added |
| `webpages:Version` | `3.0.0.0` | All | Web.config appSettings |
| `webpages:Enabled` | `false` | All | Web.config appSettings |
| `ClientValidationEnabled` | `true` | All | Web.config appSettings |
| `UnobtrusiveJavaScriptEnabled` | `true` | All | Web.config appSettings |
| `system.web/compilation/@debug` | `true` (Debug), removed (Release) | Debug / Release | XDT transform |
| `system.web/httpRuntime/@targetFramework` | `4.7.2` | All | Web.config |

### DataLoader (`App.config`)

| Property Key | Default / Current Value | Profile | Source |
|---|---|---|---|
| `TargetSearchServiceName` | `[TARGET SEARCH SERVICE - Excluding search.windows.net]` (placeholder) | All | App.config appSettings |
| `TargetSearchServiceApiKey` | `[TARGET SEARCH SERVICE API KEY]` (placeholder) | All | App.config appSettings |
| `startup/supportedRuntime` | `v4.0 / .NETFramework,Version=v4.5` | All | App.config startup |

> Note: `NYCJobsWeb/JobsSearch.cs` reads `ConfigurationManager.AppSettings["Searchendpoint"]` but no such key is defined in `Web.config`. This is a bug — the code reads a missing key and will receive `null`, causing a `NullReferenceException` in `new Uri(searchendpoint)` at startup. The key `SearchServiceName` is defined but never read by the application code.

## Startup Parameters & Resource Requirements

| Service | Runtime Options | Memory / CPU | Instance Count | Notes |
|---|---|---|---|---|
| NYCJobsWeb | None configured; IIS worker process defaults | IIS default (varies by App Pool config) | 1 (IIS-managed) | No explicit heap, CPU affinity, or worker process count configured |
| DataLoader | None; standard .NET Framework console app | OS default | 1 (manual CLI invocation) | Run once at index setup time |

No Docker, Kubernetes, or cloud deployment manifests are present. No JVM `-Xms`/`-Xmx`, `ASPNETCORE_*` environment variables, or platform-level resource limits are defined.

## Startup Dependency Chain

```
NYCJobsWeb
  └── depends on: Azure AI Search service (nycjobs + zipcodes indexes populated)
       └── prerequisite: DataLoader must be run manually before first use

DataLoader (run-once, offline)
  └── depends on: Azure AI Search service endpoint accessible + Admin API key valid
```

No automated startup ordering mechanism (Docker Compose `depends_on`, Kubernetes readiness probes, `dockerize` wait-for-TCP) is configured. DataLoader is a manual prerequisite with no health-check integration. If the Azure AI Search service is unavailable at NYCJobsWeb startup, the static `SearchClient` initialization in `JobsSearch` catches the exception silently — the app starts but returns `null` on all search calls.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Location | Storage |
|---|---|---|---|
| `SearchServiceApiKey` | Azure AI Search Query API Key | `NYCJobsWeb/Web.config` appSettings | Plain text in source-controlled XML file |
| `BingApiKey` | Bing Maps / Geocoding API Key | `NYCJobsWeb/Web.config` appSettings (two entries, second is empty) | Plain text in source-controlled XML file |
| `TargetSearchServiceApiKey` | Azure AI Search Admin API Key | `DataLoader/DataLoader/App.config` appSettings | Plain text in source-controlled XML file |
| `TargetSearchServiceName` | Azure AI Search service hostname | `DataLoader/DataLoader/App.config` appSettings | Plain text in source-controlled XML file |

### Secrets Provisioning Workflow

**Current state (insecure)**: All secrets are embedded as plain-text values in committed `Web.config` / `App.config` XML files. There is no secrets manager, no KeyVault reference, no environment-variable injection, and no `.gitignore` exclusion for these files. The current values in the repository appear to be placeholders (`[ENTER BING API KEY]`, `<api-key>`) rather than real credentials, suggesting the demo relies on the user replacing them manually before running.

**Recommended target workflow**:
1. Store secrets in **Azure Key Vault** (for deployed app) or **user secrets** (`dotnet user-secrets`) for local development
2. Reference secrets via **Azure Managed Identity** from App Service (no stored credentials needed)
3. For DataLoader, pass secrets via **environment variables** or **Azure CLI** at invocation time
4. Add `Web.config` and `App.config` to `.gitignore` or replace credential entries with `$(ENV_VAR)` MSBuild substitution

## Feature Flags

No feature flag framework detected. There are no LaunchDarkly, Unleash, .NET `Microsoft.FeatureManagement`, `@ConditionalOnProperty`, or any other feature-toggle mechanism configured. No A/B testing or gradual rollout infrastructure is present.

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| Target Framework (NYCJobsWeb) | .NET Framework 4.7.2 | `NYCJobsWeb.csproj` `<TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>` |
| Target Framework (DataLoader) | .NET Framework 4.5 | `DataLoader.csproj` `<TargetFrameworkVersion>v4.5</TargetFrameworkVersion>` |
| ASP.NET MVC | 5.2.2 | `NYCJobsWeb/packages.config` |
| ASP.NET Razor | 3.2.2 | `NYCJobsWeb/packages.config` |
| ASP.NET WebPages | 3.2.2 | `NYCJobsWeb/packages.config` |
| Azure.Search.Documents SDK | 11.1.1 | `NYCJobsWeb/packages.config` |
| Azure.Core | 1.4.1 | `NYCJobsWeb/packages.config` |
| Newtonsoft.Json | 10.0.3 (NYCJobsWeb) / 9.0.1 (DataLoader) | `packages.config` files |
| Bootstrap | 3.4.1 | `NYCJobsWeb/packages.config` |
| jQuery | 3.1.1 | `NYCJobsWeb/packages.config` |
| BingGeocodingHelper | 1.1 | `NYCJobsWeb/packages.config` |
| Microsoft.Spatial | 7.5.3 | `NYCJobsWeb/packages.config` |
| MSBuild ToolsVersion | 12.0 | Both `.csproj` files |
| Azure Search REST API (DataLoader) | 2015-02-28-Preview | `DataLoader/AzureSearchHelper.cs` hardcoded constant |
