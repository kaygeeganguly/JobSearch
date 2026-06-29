# API & Service Communication Contracts

The NYC Jobs application exposes five HTTP endpoints through a single ASP.NET MVC 5 controller, all communicating synchronously with Azure AI Search. There are no inter-service message queues, no API gateway, and no authentication layer.

## Service Catalog

| Service | Port | Category | Purpose |
|---------|------|----------|---------|
| NYCJobsWeb | 80/443 (IIS-hosted) | API Layer + UI | ASP.NET MVC 5 web application serving the job search UI and JSON API actions |
| DataLoader | N/A (console) | Infrastructure | One-time console tool for creating Azure AI Search indices and bulk-importing NYC Jobs data |

## API Endpoints Inventory

| Service | Method | Path | Request Type | Response Type |
|---------|--------|------|-------------|---------------|
| NYCJobsWeb / HomeController | GET | `/` or `/Home/Index` | None | HTML (Index view) |
| NYCJobsWeb / HomeController | GET | `/Home/JobDetails` | None | HTML (JobDetails view) |
| NYCJobsWeb / HomeController | GET | `/Home/Search` | Query params: `q`, `businessTitleFacet`, `postingTypeFacet`, `salaryRangeFacet`, `sortType`, `lat`, `lon`, `currentPage`, `zipCode`, `maxDistance` | JSON — `NYCJob` |
| NYCJobsWeb / HomeController | GET | `/Home/Suggest` | Query params: `term` (string), `fuzzy` (bool, default true) | JSON — `List<string>` |
| NYCJobsWeb / HomeController | GET | `/Home/LookUp` | Query param: `id` (string) | JSON — `NYCJobLookup` |

> Note: URL routing follows the default ASP.NET MVC convention `{controller}/{action}/{id}`. No custom route prefixes or API versioning are in use.

## Management & Observability Endpoints

| Service | Endpoint | Notes |
|---------|----------|-------|
| NYCJobsWeb | None | No health check, readiness probe, liveness probe, or Swagger/OpenAPI endpoint is configured |
| DataLoader | None | Console-only tool; no web endpoints |

No custom metrics, Application Insights instrumentation, or diagnostic middleware is configured in either project.

## DTOs & Contracts

The application defines two response model classes in the `NYCJobsWeb.Models` namespace:

- **`NYCJob`** — response DTO returned by the `Search` action. Carries the full Azure AI Search result set: a `Results` collection (`IList<SearchResult<SearchDocument>>`), a `Facets` dictionary, and a total `Count`. This is a service-level model wrapping Azure SDK types directly; there is no custom mapping or separation between the domain model and the API contract.
- **`NYCJobLookup`** — response DTO returned by the `LookUp` action. Holds a single `SearchDocument` representing a resolved job posting.

Both classes are mutable reference types (standard C# classes). No immutability (`record`, `readonly`, `init`) is applied. There is no OpenAPI/Swagger specification, no `.proto` file, and no GraphQL schema. Serialization is handled by ASP.NET MVC 5's built-in `JsonResult`, which uses `Newtonsoft.Json` 10.0.3 by default. The `SearchDocument` type is a dynamic dictionary from the Azure.Search.Documents SDK; field shapes are not statically typed at the contract level.

For full field details of the underlying search documents, refer to `data-architecture.md`.

## Communication Patterns

**Synchronous REST (outbound):** The `JobsSearch` class communicates synchronously with Azure AI Search using the `Azure.Search.Documents` SDK. Two `SearchClient` instances are created at application startup (static field initialization) — one for the `nycjobs` index and one for the `zipcodes` index. All SDK calls (`Search`, `Suggest`, `GetDocument`) are synchronous (`.Result`-style blocking calls on the async SDK methods are not used; the synchronous overloads are called directly).

**Synchronous REST (inbound):** All five controller actions return synchronous `ActionResult` / `JsonResult` responses over HTTP. There is no streaming, Server-Sent Events, or WebSocket support.

**Asynchronous messaging:** None. No message queue (Azure Service Bus, RabbitMQ, Kafka) is used.

**Resilience patterns:** None configured. There is no circuit breaker (Polly or similar), no retry policy, no timeout configuration, and no bulkhead pattern. Azure SDK built-in retries (exponential backoff) apply by default, but no custom retry options are set in the `SearchClientOptions`.

**Service discovery:** None. The Azure AI Search endpoint URL is hardcoded in `Web.config` (`appSettings/Searchendpoint`). No Consul, Eureka, or Azure Service Discovery is used.

**API gateway:** None. The application is a self-contained monolith; there is no gateway tier.

**Security posture:** No authentication, no authorization, and no HTTPS enforcement are configured at the application level. All five endpoints are publicly accessible with no authorization checks. The application relies on the Azure AI Search query API key (stored in `Web.config` as plain text) to restrict write access to the search index, but no user-level authentication is enforced. TLS termination is expected at the IIS/hosting level but is not enforced by the application code. There is no CSRF protection, CORS policy, or Content Security Policy middleware configured.

## Service Technology Matrix

| Service | Web Framework | Data Access | Discovery | Gateway | Health Checks | Cache | Metrics |
|---------|-------------|-------------|-----------|---------|---------------|-------|---------|
| NYCJobsWeb | ASP.NET MVC 5 | Azure.Search.Documents SDK (direct) | None | None | None | None | None |
| DataLoader | None (console) | Azure Search REST API (HttpClient) | None | None | None | None | None |

## Service Communication Sequence

```mermaid
sequenceDiagram
    participant Browser as "Web Browser"
    participant Home as "HomeController"
    participant Jobs as "JobsSearch"
    participant AzSearch as "Azure AI Search"
    participant BingGeo as "Bing Geocoding API"

    Browser->>Home: GET /Home/Search?q=analyst&zipCode=10001&maxDistance=5
    Home->>Jobs: SearchZip("10001")
    Jobs->>AzSearch: Search zipcodes index (zip=10001)
    AzSearch-->>Jobs: geo_location (lat/lon)
    Jobs-->>Home: zipResponse (lat, lon)
    Home->>Jobs: Search(q, facets, sortType, lat, lon, page, maxDistance)
    Jobs->>AzSearch: Search nycjobs index (filter, facets, highlights)
    AzSearch-->>Jobs: SearchResults with facets and highlights
    Jobs-->>Home: SearchResults
    Home-->>Browser: 200 JSON NYCJob (results, facets, count)

    Browser->>Home: GET /Home/Suggest?term=ana&fuzzy=true
    Home->>Jobs: Suggest("ana", fuzzy=true)
    Jobs->>AzSearch: Suggest nycjobs index (suggester=sg)
    AzSearch-->>Jobs: SuggestResults
    Jobs-->>Home: SuggestResults
    Home-->>Browser: 200 JSON List of suggestion strings

    Browser->>Home: GET /Home/LookUp?id=12345
    Home->>Jobs: LookUp("12345")
    Jobs->>AzSearch: GetDocument nycjobs index (id=12345)
    AzSearch-->>Jobs: SearchDocument
    Jobs-->>Home: SearchDocument
    Home-->>Browser: 200 JSON NYCJobLookup

    Note over Home,BingGeo: Bing Geocoding API referenced via BingGeocodingHelper\nbut geo-lookup for zip codes is handled by Azure AI Search;\nBingGeocoder class is imported but geo-resolution\nuses zipcodes index in practice.
```
