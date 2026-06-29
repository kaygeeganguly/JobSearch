# Assessment Overview

This document serves as the navigation entry point for the supplementary architecture and analysis documents generated for the **NYC Jobs Search** application (a .NET Framework 4.7.2 ASP.NET MVC 5 job portal backed by Azure AI Search).

## Supplementary Documents

| Document | Description |
|----------|-------------|
| [Architecture Diagram](./architecture-diagram.md) | Two-layer visualization of the application architecture: high-level component diagram (application layers, external services, data flow) and detailed component relationship diagram (controllers, services, models, views). |
| [Dependency Map](./dependency-map.md) | Visual map of all external NuGet package dependencies grouped by functional category (Web Frameworks, Azure AI Search, Geo/Spatial, Frontend, Utilities, Runtime Polyfills) with version and compatibility risk analysis. |
| [API & Service Communication Contracts](./api-service-contracts.md) | Inventory of all HTTP endpoints, service catalog, DTOs and response models, communication patterns (synchronous REST, no async messaging), security posture, and a Mermaid sequence diagram of the primary request flow. |
| [Data Architecture & Persistence Layer](./data-architecture.md) | Documentation of the Azure AI Search index schemas (`nycjobs` and `zipcodes`), data ownership boundaries, key query methods, caching strategy (none configured), and data classification analysis. |
| [Configuration & Externalized Settings Inventory](./configuration-inventory.md) | Comprehensive inventory of all configuration sources (`Web.config`, `App.config`, XDT transforms), build profiles, runtime profiles, property keys and values, secrets handling, and framework version matrix. |
| [Core Business Workflows](./business-workflows.md) | End-to-end documentation of the four primary business workflows (full-text job search with faceting, auto-suggest, job detail lookup, and index initialization), business rules, decision logic, and a Mermaid sequence diagram. |
