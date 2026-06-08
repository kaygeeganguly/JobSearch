# Configuration & Externalized Settings Inventory

The NYC Jobs Search solution uses two XML-based configuration files (`Web.config` and `App.config`) as its sole configuration sources. There is no environment-specific profile system, secret store integration, or externalized configuration service.

## Configuration Sources

| Source | Type | Path / Location | Notes |
|---|---|---|---|
| `Web.config` | XML `<appSettings>` | `NYCJobsWeb/Web.config` | Primary configuration for the ASP.NET MVC web app; contains search endpoint, API key, and Bing API key |
| `Web.Debug.config` | XML transform | `NYCJobsWeb/Web.Debug.config` | Debug-mode transform applied during Debug builds |
| `Web.Release.config` | XML transform | `NYCJobsWeb/Web.Release.config` | Release-mode transform applied during Release builds |
| `App.config` | XML `<appSettings>` | `DataLoader/DataLoader/App.config` | Configuration for the DataLoader console app; contains target search service name and admin API key |

No Spring Cloud Config, Azure App Configuration, AWS AppConfig, Consul KV, HashiCorp Vault, Azure Key Vault, `.env` files, `appsettings.json`, `launchSettings.json`, Kubernetes ConfigMaps, or Docker Compose environment sections are present.

## Build Profiles

| Profile | Activation | Purpose | Key Differences |
|---|---|---|---|
| Debug | Default in Visual Studio / `dotnet build` without `-c Release` | Development and local testing builds | `compilation debug="true"` in `Web.config`; `Web.Debug.config` transform applied |
| Release | Manual: `-c Release` or publish pipeline | Production deployment packaging | `Web.Release.config` transform applied; typically disables debug mode and may substitute config values |

The `Web.Debug.config` and `Web.Release.config` transforms use the XML Document Transform (XDT) syntax. They are applied to `Web.config` at publish time. The current transforms appear to be scaffolded defaults (no custom transforms have been added beyond the Visual Studio-generated placeholders).

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| (Single environment) | No runtime profile system configured | `Web.config` / `App.config` | N/A — all settings are statically in the single config file |

There is no `ASPNETCORE_ENVIRONMENT`, `spring.profiles.active`, or equivalent environment-switching mechanism. The application targets legacy ASP.NET MVC 5 on .NET Framework 4.7.2, which uses `Web.config` directly and does not support the environment-based `appsettings.{Environment}.json` pattern.

## Properties Inventory

### NYCJobsWeb (`Web.config`)

| Property Key | Default Value | Source | Notes |
|---|---|---|---|
| `Searchendpoint` | (not set — `SearchServiceName` key used instead) | `Web.config` `<appSettings>` | Used by `JobsSearch` as the Azure AI Search endpoint URL |
| `SearchServiceName` | `azs-playground` | `Web.config` `<appSettings>` | Placeholder service name; must be replaced before deployment |
| `SearchServiceApiKey` | `<api-key>` | `Web.config` `<appSettings>` | Azure AI Search query API key — placeholder value committed to source control |
| `BingApiKey` | `""` (empty string, declared twice) | `Web.config` `<appSettings>` | Bing Maps geocoding API key; duplicate key declaration present in file |
| `webpages:Version` | `3.0.0.0` | `Web.config` `<appSettings>` | ASP.NET WebPages version |
| `webpages:Enabled` | `false` | `Web.config` `<appSettings>` | Disables WebPages routing |
| `ClientValidationEnabled` | `true` | `Web.config` `<appSettings>` | Enables ASP.NET MVC client-side validation |
| `UnobtrusiveJavaScriptEnabled` | `true` | `Web.config` `<appSettings>` | Enables unobtrusive JavaScript validation |
| `system.web/compilation/@debug` | `true` | `Web.config` `<system.web>` | Enables debug compilation; should be `false` in production |
| `system.web/compilation/@targetFramework` | `4.7.2` | `Web.config` `<system.web>` | Target .NET Framework version |
| `system.web/httpRuntime/@targetFramework` | `4.7.2` | `Web.config` `<system.web>` | HTTP runtime target framework |

### DataLoader (`App.config`)

| Property Key | Default Value | Source | Notes |
|---|---|---|---|
| `TargetSearchServiceName` | `[TARGET SEARCH SERVICE - Excluding search.windows.net]` | `App.config` `<appSettings>` | Azure AI Search service hostname prefix; placeholder must be replaced |
| `TargetSearchServiceApiKey` | `[TARGET SEARCH SERVICE API KEY]` | `App.config` `<appSettings>` | Azure AI Search admin API key; placeholder must be replaced before running |

## Startup Parameters & Resource Requirements

| Service | Runtime Options | Memory Allocation | Instance Count | Notes |
|---|---|---|---|---|
| NYCJobsWeb | IIS application pool (no explicit JVM or .NET runtime flags) | Not configured | 1 (IIS-managed) | Hosted as an IIS web application; no `web.config` process model or memory limit configuration present |
| DataLoader | .NET Framework 4.5+ CLR | Not configured | 1 (manual run) | Console application; run once for initial data load; no automated scheduling |

No `-Xms`/`-Xmx` JVM flags, Docker resource limits, Kubernetes resource requests/limits, or `ASPNETCORE_ENVIRONMENT` environment variable overrides are configured.

## Startup Dependency Chain

The application has no automated startup dependency management. The expected manual startup order is:

1. **DataLoader** must be run first (once) to create and populate the `nycjobs` and `zipcodes` Azure AI Search indexes. It has no health checks or readiness probes — it simply exits when done.
2. **NYCJobsWeb** can then be started. It fails silently at startup if the Azure AI Search endpoint or API key are incorrect (`JobsSearch` catches the exception and stores it in `errorMessage`), but the IIS application pool will start regardless.

There are no Docker Compose `depends_on` conditions, Kubernetes readiness probes, Spring Cloud Config retry policies, or `dockerize` wait mechanisms in place.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage | Risk |
|---|---|---|---|
| `SearchServiceApiKey` in `Web.config` | Azure AI Search query API key | Plaintext in source-controlled XML file (`[MASKED]`) | **HIGH** — API key committed to version control; value `<api-key>` is a placeholder but the pattern risks real keys being committed |
| `BingApiKey` in `Web.config` | Bing Maps REST API key | Plaintext in source-controlled XML file (currently empty string) | **MEDIUM** — key slot exists in source control; any real key added here would be exposed |
| `TargetSearchServiceApiKey` in `App.config` | Azure AI Search admin API key | Plaintext in source-controlled XML file (`[MASKED]`) | **HIGH** — admin API key (write access to indexes) stored in source control as placeholder |

### Secrets Provisioning Workflow

There is no secrets provisioning workflow. All secrets are currently stored as plaintext values in XML configuration files that are committed to the source repository. The placeholders (`<api-key>`, `[TARGET SEARCH SERVICE API KEY]`) indicate the intent to substitute real values, but no mechanism for secure injection is in place:

- No environment variable substitution at runtime
- No Azure Key Vault or Managed Identity integration
- No CI/CD pipeline secret injection (e.g., GitHub Actions secrets → environment variables)
- No encryption (Jasypt, DPAPI, or similar)

For production deployment, secrets should be moved out of source control and injected via environment variables, Azure Key Vault references (in `appsettings.json` with Managed Identity), or a deployment pipeline.

## Feature Flags

No feature flag frameworks, `@ConditionalOnProperty` annotations, `.NET FeatureManagement` integration, LaunchDarkly, Unleash, or custom toggle patterns are present in the solution.

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| Target Runtime | .NET Framework 4.7.2 | `Web.config` `targetFramework="4.7.2"`, NYCJobsWeb.csproj |
| Target Runtime (DataLoader) | .NET Framework 4.5 | `App.config` `<supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5">` |
| ASP.NET MVC | 5.2.2 | `packages.config` `Microsoft.AspNet.Mvc 5.2.2` |
| ASP.NET Razor | 3.2.2 | `packages.config` `Microsoft.AspNet.Razor 3.2.2` |
| ASP.NET WebPages | 3.2.2 | `packages.config` `Microsoft.AspNet.WebPages 3.2.2` |
| Azure.Search.Documents | 11.1.1 | `packages.config` |
| Azure.Core | 1.4.1 | `packages.config` |
| Microsoft.Spatial | 7.5.3 | `packages.config` |
| BingGeocodingHelper | 1.1 | `packages.config` |
| Newtonsoft.Json (web) | 10.0.3 | `packages.config` (NYCJobsWeb) |
| Newtonsoft.Json (loader) | 9.0.1 | `packages.config` (DataLoader) |
| Bootstrap | 3.4.1 | `packages.config` |
| jQuery | 3.1.1 | `packages.config` |
| Build Tool | MSBuild (Visual Studio / dotnet CLI) | NYCJobsWeb.sln |
