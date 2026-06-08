# Assessment Overview

This directory contains supplementary analysis documents generated for the NYC Jobs Search solution (`NYCJobsWeb` + `DataLoader`). Each document provides a focused view of a specific architectural or operational concern to support the cloud migration assessment.

## Supplementary Documents

| Document | Description |
|----------|-------------|
| [architecture-diagram.md](architecture-diagram.md) | Two-layer architecture visualization: high-level application architecture diagram (technology stack, data storage, external services) and detailed component relationship diagram (controllers, services, models, data access clients) |
| [dependency-map.md](dependency-map.md) | Visual map of all external NuGet package dependencies grouped by functional category (Web Frameworks, Search/Cloud SDK, UI libraries, serialization, runtime compatibility shims), including version risks and compatibility observations |
| [api-service-contracts.md](api-service-contracts.md) | Inventory of all HTTP endpoints exposed by `HomeController`, DTO/contract classes, communication patterns (synchronous REST to Azure AI Search), security posture, and a service communication sequence diagram |
| [data-architecture.md](data-architecture.md) | Azure AI Search index schema documentation for the `nycjobs` and `zipcodes` indexes, data ownership boundaries, key data access methods in `JobsSearch`, caching strategy (none), and data sensitivity analysis |
| [configuration-inventory.md](configuration-inventory.md) | Complete inventory of all configuration sources (`Web.config`, `App.config`), build profiles (Debug/Release), property keys and placeholder values, secrets handling workflow, and framework/runtime version catalog |
| [business-workflows.md](business-workflows.md) | End-to-end documentation of the four core business workflows (job search with geo-filter, autocomplete suggestions, job detail lookup, and index data loading), domain entities, business rules, and decision logic |
