# Architecture Diagram

This document summarizes the application's high-level architecture and the main component relationships across the web app and data-loader utility.

## Application Architecture

```mermaid
flowchart TD
    subgraph Client["Client Layer"]
        Browser["Web Browser"]
    end
    subgraph App["Application Layer - ASP.NET MVC 5 on .NET Framework 4.7.2"]
        Controller["HomeController"]
        SearchService["JobsSearch Service"]
        ViewLayer["Razor Views"]
    end
    subgraph Data["Data Layer"]
        SearchIndex[("Azure Cognitive Search index nycjobs")]
        ZipIndex[("Azure Cognitive Search index zipcodes")]
        JsonSeed[("Schema and Data JSON files")]
    end
    subgraph External["External Services"]
        AzureSearch["Azure Cognitive Search endpoint"]
        BingApi["Bing Geocoding API"]
    end

    Browser -->|"HTTP requests"| Controller
    Controller -->|"render pages"| ViewLayer
    Controller -->|"query and suggest"| SearchService
    SearchService -->|"document search"| SearchIndex
    SearchService -->|"zip distance lookup"| ZipIndex
    SearchIndex -->|"hosted on"| AzureSearch
    ZipIndex -->|"hosted on"| AzureSearch
    Controller -->|"geocoding key usage"| BingApi
    JsonSeed -->|"loaded by DataLoader"| SearchIndex
    JsonSeed -->|"loaded by DataLoader"| ZipIndex
```

### Technology Stack Summary

| Layer | Technology | Version | Purpose |
|---|---|---|---|
| Presentation | ASP.NET MVC + Razor | MVC 5.2.2 | Server-rendered UI and JSON endpoints |
| Business Logic | C# service class (`JobsSearch`) | .NET Framework 4.7.2 | Query orchestration and filter composition |
| Data Access | Azure.Search.Documents SDK | 11.1.1 | Access Azure Cognitive Search indexes |
| Utility Tooling | Console app (`DataLoader`) | .NET Framework 4.5 | Recreate indexes and bulk import sample data |

### Data Storage & External Services

The application does not use a relational database in-repo. It relies on Azure Cognitive Search indexes (`nycjobs`, `zipcodes`) as the primary data store and uses external search credentials from configuration. A Bing API key is also configured for geospatial lookup support.

### Key Architectural Decisions

- Uses thin MVC controllers that delegate search logic to a single `JobsSearch` service.
- Treats Azure Cognitive Search as both query engine and document store for demo data.
- Keeps index provisioning and seed upload in a separate console utility (`DataLoader`).

## Component Relationships

```mermaid
flowchart LR
    subgraph Presentation["Presentation"]
        HomeCtrl["HomeController"]
        RazorViews["Razor Views"]
    end
    subgraph Business["Business Logic"]
        JobSvc["JobsSearch"]
    end
    subgraph DataAccess["Data Access"]
        SearchClient["SearchClient nycjobs"]
        ZipClient["SearchClient zipcodes"]
    end
    subgraph Infra["Infrastructure"]
        RouteCfg["RouteConfig"]
        GlobalApp["Global.asax startup"]
        WebCfg["Web.config appSettings"]
    end

    GlobalApp -->|"registers"| RouteCfg
    RouteCfg -->|"routes to"| HomeCtrl
    HomeCtrl -->|"returns"| RazorViews
    HomeCtrl -->|"search, suggest, lookup"| JobSvc
    JobSvc -->|"queries"| SearchClient
    JobSvc -->|"queries"| ZipClient
    WebCfg -.->|"endpoint and keys"| JobSvc
```

### Component Inventory

| Component | Layer | Type | Responsibility |
|---|---|---|---|
| HomeController | Presentation | MVC Controller | Handles page rendering and JSON actions for search/suggest/lookup |
| JobsSearch | Business Logic | Service class | Builds options/filters and calls Azure Search SDK |
| SearchClient (nycjobs) | Data Access | SDK client | Executes job search, suggest, and lookup operations |
| SearchClient (zipcodes) | Data Access | SDK client | Resolves zip code coordinates for distance filtering |
| RouteConfig | Infrastructure | Routing config | Defines default MVC route template |
| Global.asax | Infrastructure | App startup | Registers routes at application startup |
| Web.config | Infrastructure | Configuration source | Stores search endpoint and API keys |
