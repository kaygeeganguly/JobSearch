# Dependency Map

This document summarizes declared external dependencies for the solution projects (NYCJobsWeb and DataLoader), with focus on runtime dependencies used by the application.

## Dependencies

```mermaid
flowchart LR
    App["JobSearch Solution"]

    subgraph Web["Web Frameworks"]
        AspMvc["Microsoft.AspNet.Mvc 5.2.2"]
        Razor["Microsoft.AspNet.Razor 3.2.2"]
        WebPages["Microsoft.AspNet.WebPages 3.2.2"]
    end
    subgraph Data["Database / ORM"]
        SearchSdk["Azure.Search.Documents 11.1.1"]
        AzureCore["Azure.Core 1.4.1"]
    end
    subgraph Log["Logging"]
        NoLogging["No dedicated logging library"]
    end
    subgraph Sec["Security"]
        KeyCred["AzureKeyCredential via Azure SDK"]
    end
    subgraph Util["Utilities"]
        Json10["Newtonsoft.Json 10.0.3"]
        Json9["Newtonsoft.Json 9.0.1"]
        BingGeo["BingGeocodingHelper 1.1"]
        TextJson["System.Text.Json 4.6.0"]
    end

    App -->|"web"| Web
    App -->|"search sdk"| Data
    App -->|"security"| Sec
    App -->|"utilities"| Util
    App -->|"logging"| Log
    Json10 -.->|"version split across projects"| Json9
```

### Dependency Summary

| Category | Count | Key Libraries | Notes |
|---|---:|---|---|
| Web Frameworks | 3 | Microsoft.AspNet.Mvc, Razor, WebPages | Legacy ASP.NET MVC 5 stack |
| Database / ORM | 2 | Azure.Search.Documents, Azure.Core | Uses Azure Search service instead of local DB ORM |
| Security | 1 | AzureKeyCredential (Azure SDK) | API-key based service auth |
| Utilities | 4 | Newtonsoft.Json, BingGeocodingHelper, System.Text.Json | Mixed old/new JSON libraries |
| Logging | 0 | None | No explicit logging framework package |

### Version & Compatibility Risks

Core app projects target .NET Framework 4.7.2 and 4.5, and several packages are comparatively old (for example Newtonsoft.Json 9.x/10.x split and legacy ASP.NET MVC packages). This increases upgrade effort for .NET 10 modernization and can require API and compatibility remapping.

### Notable Observations

- Dependency set is NuGet `packages.config` based, not SDK-style package references.
- Two projects use different Newtonsoft.Json major baselines (9.0.1 and 10.0.3).
- No resilience/observability packages (Polly, OpenTelemetry, Serilog, etc.) are declared.
- Front-end libraries (Bootstrap/jQuery) are pinned to older versions suitable for legacy MVC views.

## Test Dependencies

| Framework | Version | Notes |
|---|---|---|
| None detected | N/A | No test framework packages found in project package manifests |

Total test-scope dependencies: 0
No dedicated test dependency declarations were detected in the repository build manifests.
