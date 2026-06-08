# Architecture Diagram

This repository contains a legacy ASP.NET MVC web application for searching NYC jobs and a companion console loader used to seed Azure AI Search indexes.

## Application Architecture

```mermaid
flowchart TD
    subgraph Client["Client Layer"]
        Browser["Web Browser"]
    end

    subgraph Web["Application Layer - ASP.NET MVC 5 on .NET Framework 4.7.2"]
        HomeCtrl["HomeController"]
        JobsSvc["JobsSearch service class"]
    end

    subgraph Loader["Data Ingestion Layer - .NET Framework Console"]
        LoaderApp["DataLoader Program"]
        SearchHelper["AzureSearchHelper REST helper"]
    end

    subgraph Data["Data Layer"]
        SearchIdx[("Azure AI Search index: nycjobs")]
        ZipIdx[("Azure AI Search index: zipcodes")]
        JsonSeed[("Schema and JSON seed files")]
    end

    subgraph External["External Services"]
        BingGeo["Bing Geocoding API"]
        AzureSearch["Azure AI Search service"]
    end

    Browser -->|"HTTP requests"| HomeCtrl
    HomeCtrl -->|"query and lookup"| JobsSvc
    JobsSvc -->|"search and suggest"| SearchIdx
    JobsSvc -->|"zip distance lookup"| ZipIdx
    HomeCtrl -->|"address geocoding"| BingGeo
    LoaderApp -->|"create indexes and upload docs"| SearchHelper
    SearchHelper -->|"REST API calls"| AzureSearch
    LoaderApp -->|"reads schema and data"| JsonSeed
    AzureSearch -->|"hosts"| SearchIdx
    AzureSearch -->|"hosts"| ZipIdx
```

### Technology Stack Summary

| Layer | Technology | Version | Purpose |
|---|---|---|---|
| Presentation | ASP.NET MVC | 5.2.2 | Server-rendered UI and JSON endpoints |
| Search Integration | Azure.Search.Documents | 11.1.1 | Querying, suggestion, and document lookup |
| Geocoding | BingGeocodingHelper | 1.1 | Resolving location data for distance-based filtering |
| Data Ingestion | .NET Console App | .NET Framework 4.5 | Recreates indexes and bulk-loads seed data |
| Data Store | Azure AI Search | External service | Stores nycjobs and zipcodes search documents |

### Data Storage & External Services

The application uses Azure AI Search as its primary data store, with separate indexes for jobs and zip codes. The web app queries the indexes for search, suggestions, and detail lookups, while the DataLoader console app seeds these indexes from local schema and JSON files. Bing geocoding is used by the web layer for location-related search behavior.

### Key Architectural Decisions

- Uses a thin MVC controller with a dedicated `JobsSearch` integration class for search operations.
- Separates one-time/batch ingestion concerns into a standalone `DataLoader` executable.
- Relies on managed external search infrastructure (Azure AI Search) instead of local relational persistence.

## Component Relationships

```mermaid
flowchart LR
    subgraph Presentation
        HomeController["HomeController"]
        Views["Razor Views"]
    end

    subgraph Business["Business Logic"]
        JobsSearch["JobsSearch"]
    end

    subgraph DataAccess["Data Access"]
        SearchClient["SearchClient nycjobs"]
        ZipClient["SearchClient zipcodes"]
        JobDto["NYCJob / NYCJobLookup DTOs"]
    end

    subgraph Infrastructure
        RouteConfig["RouteConfig"]
        Config["Web.config AppSettings"]
        LoaderProgram["DataLoader Program"]
        LoaderHelper["AzureSearchHelper"]
    end

    HomeController -->|"renders pages"| Views
    HomeController -->|"delegates search"| JobsSearch
    JobsSearch -->|"query"| SearchClient
    JobsSearch -->|"zip query"| ZipClient
    HomeController -->|"returns"| JobDto
    RouteConfig -.->|"routes"| HomeController
    Config -.->|"provides keys and endpoint"| JobsSearch
    LoaderProgram -->|"uses"| LoaderHelper
    LoaderProgram -->|"loads schema/data"| SearchClient
    LoaderProgram -->|"loads schema/data"| ZipClient
```

### Component Inventory

| Component | Layer | Type | Responsibility |
|---|---|---|---|
| HomeController | Presentation | MVC Controller | Handles page rendering and JSON search endpoints |
| Razor Views | Presentation | View templates | Displays index and job detail pages |
| JobsSearch | Business Logic | Service/helper class | Builds search options and executes Azure Search operations |
| SearchClient (nycjobs) | Data Access | SDK client | Executes job search, suggest, and lookup operations |
| SearchClient (zipcodes) | Data Access | SDK client | Resolves zip code coordinates for distance filtering |
| NYCJob / NYCJobLookup | Data Access | DTOs | Shapes JSON responses returned by controller actions |
| RouteConfig | Infrastructure | MVC routing config | Defines default route pattern |
| Web.config AppSettings | Infrastructure | Config source | Stores endpoint and API keys for search/geocoding services |
| DataLoader Program | Infrastructure | Console entry point | Recreates indexes and uploads seed documents |
| AzureSearchHelper | Infrastructure | HTTP helper | Sends authenticated REST requests to Azure Search |
