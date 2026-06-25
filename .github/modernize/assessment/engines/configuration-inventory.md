# Configuration & Externalized Settings Inventory

The NYC Jobs solution has a minimal configuration landscape: two XML-based config files (`Web.config` and `App.config`) with flat `<appSettings>` key-value pairs, no runtime profiles, no secret store integration, and all sensitive values stored as plain text placeholders.

## Configuration Sources

| Source | Type | Path / Location | Notes |
|--------|------|----------------|-------|
| Web.config | XML application config | `NYCJobsWeb/Web.config` | ASP.NET Framework config; holds Azure Search endpoint/key, Bing API key, MVC/routing settings, assembly binding redirects |
| Web.Debug.config | XML transform | `NYCJobsWeb/Web.Debug.config` | XDT transform applied during Debug build; no overrides defined by default |
| Web.Release.config | XML transform | `NYCJobsWeb/Web.Release.config` | XDT transform applied during Release build; no overrides defined by default |
| Views/web.config | XML | `NYCJobsWeb/Views/web.config` | Razor view engine configuration only |
| App.config | XML application config | `DataLoader/DataLoader/App.config` | Console app config; holds target Azure Search service name and admin API key |

No `appsettings.json`, `launchSettings.json`, `bootstrap.yml`, `.env` files, Spring Cloud Config server, Azure App Configuration, Kubernetes ConfigMaps, Docker Compose environment sections, or any external configuration repository is present.

## Build Profiles

| Profile | Activation | Purpose | Key Changes |
|---------|-----------|---------|-------------|
| Debug | Default in Visual Studio / `Configuration=Debug` | Development builds with full debug symbols | `<DebugSymbols>true</DebugSymbols>`, `<Optimize>false</Optimize>`, `Web.Debug.config` XDT transform applied |
| Release | Manual (`Configuration=Release`) or CI/CD pipeline | Production-ready build with optimizations | `<Optimize>true</Optimize>`, `<DebugType>pdbonly</DebugType>`, `Web.Release.config` XDT transform applied |

No Maven/Gradle profiles, MSBuild conditional compilation symbols, webpack/vite configurations, or package-level profile differentiation are defined. Both XDT transform files (`Web.Debug.config`, `Web.Release.config`) contain only the default empty transform stubs — no actual property overrides are applied.

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---------|-----------------|-------------|--------------|
| None | — | `Web.config` only | — |

There are no runtime profiles (`ASPNETCORE_ENVIRONMENT`, `spring.profiles.active`, `.env.production`, or equivalent). All runtime configuration is read from a single flat `Web.config` / `App.config` file regardless of environment. There is no mechanism to switch between development, staging, and production configuration without editing the config files directly.

## Properties Inventory

### NYCJobsWeb (`Web.config` — `<appSettings>`)

| Property Key | Default / Current Value | Profiles | Source |
|-------------|------------------------|---------|--------|
| `Searchendpoint` | *(not set — key uses `SearchServiceName` pattern in `Web.config` but endpoint is referenced in `JobsSearch.cs`)* | All | Web.config |
| `SearchServiceName` | `"azs-playground"` | All | Web.config |
| `SearchServiceApiKey` | `"<api-key>"` (placeholder) | All | Web.config |
| `BingApiKey` | `""` (empty — appears twice, second entry overrides first) | All | Web.config |
| `webpages:Version` | `"3.0.0.0"` | All | Web.config |
| `webpages:Enabled` | `"false"` | All | Web.config |
| `ClientValidationEnabled` | `"true"` | All | Web.config |
| `UnobtrusiveJavaScriptEnabled` | `"true"` | All | Web.config |

**Note:** The `Web.config` has a duplicate `BingApiKey` entry (`[ENTER BING API KEY]` and `""`). The second entry wins at runtime, resulting in an empty Bing API key. Geocoding will fail silently unless the key is set.

### NYCJobsWeb (`Web.config` — `<system.web>`)

| Property Key | Default / Current Value | Notes |
|-------------|------------------------|-------|
| `compilation/@debug` | `"true"` | Should be `false` in production |
| `httpRuntime/@targetFramework` | `"4.7.2"` | Target framework version |

### DataLoader (`App.config` — `<appSettings>`)

| Property Key | Default / Current Value | Profiles | Source |
|-------------|------------------------|---------|--------|
| `TargetSearchServiceName` | `"[TARGET SEARCH SERVICE - Excluding search.windows.net]"` (placeholder) | All | App.config |
| `TargetSearchServiceApiKey` | `"[TARGET SEARCH SERVICE API KEY]"` (placeholder) | All | App.config |

## Startup Parameters & Resource Requirements

| Service | Runtime Options | Memory / CPU | Instance Config |
|---------|----------------|-------------|----------------|
| NYCJobsWeb | Hosted by IIS / IIS Express; no explicit JVM/CLR heap settings configured | Not specified; depends on IIS application pool settings | Single instance; no scale-out config |
| DataLoader | Supports .NET Framework v4.0 CLR (declared via `<supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5">`); console app with no startup parameters | Not specified | Single-run CLI; not a long-running service |

No Docker Compose files, Kubernetes manifests, or cloud deployment configurations (Azure App Service `web.config` settings, ARM templates) are present in the repository.

## Startup Dependency Chain

| Order | Service | Waits For | Mechanism | Notes |
|-------|---------|----------|-----------|-------|
| 1 | Azure AI Search | — | External managed service | Must be provisioned and running before either app can function |
| 2 | DataLoader | Azure AI Search available | None (fails with `HttpRequestException` if service unreachable) | Must be run once to seed `nycjobs` and `zipcodes` indexes before the web app can return results |
| 3 | NYCJobsWeb | Azure AI Search available, indexes seeded | None (static constructor silently catches exceptions and sets `JobsSearch.errorMessage`) | App starts regardless; search returns null/errors until indexes are available |

There are no readiness probes, health checks, `dockerize` wait scripts, Kubernetes liveness probes, or Spring Cloud Config retry mechanisms. The startup dependency is entirely implicit — misconfiguration results in runtime null-reference exceptions surfaced as unhandled AJAX errors in the browser.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Current Storage | Environment |
|-----------------|------|----------------|-------------|
| `SearchServiceApiKey` (Web.config) | Azure AI Search Query API Key (read-only) | Plain text in `Web.config` — hardcoded as `"<api-key>"` placeholder | All |
| `SearchServiceName` (Web.config) | Azure AI Search service hostname prefix | Plain text in `Web.config` — set to `"azs-playground"` | All |
| `BingApiKey` (Web.config) | Bing Maps API Key | Plain text in `Web.config` — empty string (non-functional) | All |
| `TargetSearchServiceApiKey` (App.config) | Azure AI Search Admin API Key (read/write) | Plain text in `App.config` — placeholder value | All |
| `TargetSearchServiceName` (App.config) | Azure AI Search service hostname prefix | Plain text in `App.config` — placeholder value | All |

> ⚠️ The `TargetSearchServiceApiKey` in DataLoader is an **admin key** (required for index creation and document upload). Admin keys grant full read/write/delete access to the search service and must be protected more carefully than query keys.

### Secrets Provisioning Workflow

There is no automated secrets provisioning workflow. All secrets are managed manually:

1. **Developer/operator edits config files directly** — `Web.config` and `App.config` are edited by hand to insert real key values before running the application.
2. **No secrets manager** — No Azure Key Vault, AWS Secrets Manager, HashiCorp Vault, GitHub Actions secrets injection, or environment variable substitution is configured.
3. **No managed identity** — There is no Managed Identity, Service Principal, or RBAC-based access; authentication to Azure AI Search is purely API-key based.
4. **No CI/CD secrets pipeline** — The repository has no workflow files that inject secrets at build or deployment time.

This means secrets are at risk of being committed to source control if developers forget to replace the placeholder values, or if real keys are accidentally added. Recommended migration: replace `Web.config` key lookups with Azure Key Vault references and use a Managed Identity or Service Principal for authentication.

## Feature Flags

No feature flag framework is configured. There are no `@ConditionalOnProperty` annotations, .NET `IFeatureManager`, LaunchDarkly, Unleash, or custom toggle mechanisms. Conditional behavior in the application is determined purely by runtime values (e.g., empty API key causes silent failure; `maxDistance > 0` enables geo-filtering).

| Flag Name | Default | Controlled By |
|-----------|---------|--------------|
| None detected | — | — |

## Framework & Runtime Versions

| Component | Version | Source |
|-----------|---------|--------|
| .NET Framework (NYCJobsWeb) | 4.7.2 | `NYCJobsWeb/NYCJobsWeb.csproj` (`<TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>`) |
| .NET Framework (DataLoader) | 4.5 | `DataLoader/DataLoader/DataLoader.csproj` (`<TargetFrameworkVersion>v4.5</TargetFrameworkVersion>`) |
| ASP.NET MVC | 5.2.2 | `NYCJobsWeb/packages.config` |
| ASP.NET Razor | 3.2.2 | `NYCJobsWeb/packages.config` |
| ASP.NET WebPages | 3.2.2 | `NYCJobsWeb/packages.config` |
| Azure.Search.Documents | 11.1.1 | `NYCJobsWeb/packages.config` |
| Azure.Core | 1.4.1 | `NYCJobsWeb/packages.config` |
| Microsoft.Spatial | 7.5.3 | `NYCJobsWeb/packages.config` |
| BingGeocodingHelper | 1.1 | `NYCJobsWeb/packages.config` |
| Newtonsoft.Json (NYCJobsWeb) | 10.0.3 | `NYCJobsWeb/packages.config` |
| Newtonsoft.Json (DataLoader) | 9.0.1 | `DataLoader/DataLoader/packages.config` |
| Bootstrap | 3.4.1 | `NYCJobsWeb/packages.config` |
| jQuery | 3.1.1 | `NYCJobsWeb/packages.config` |
| MSBuild ToolsVersion | 12.0 | Both `.csproj` files |
| NuGet packages format | packages.config (non-SDK-style) | Both `.csproj` files |
