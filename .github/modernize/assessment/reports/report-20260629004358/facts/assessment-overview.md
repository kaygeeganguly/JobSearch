# Assessment Overview

This directory contains supplementary architecture and analysis documents generated as part of the NYCJobsWeb application assessment. Each document focuses on a specific aspect of the application's design, dependencies, and configuration.

## Supplementary Documents

| Document | Description |
|---|---|
| [Architecture Diagram](architecture-diagram.md) | Two-layer architecture visualization: high-level application architecture (ASP.NET MVC 5 + Azure AI Search) and detailed component relationship diagram with technology stack summary |
| [Dependency Map](dependency-map.md) | Visual map of all 24 external NuGet package dependencies grouped by functional category (Web Frameworks, Cloud/Search, Geospatial, Serialization, Runtime Utilities), with version risks and compatibility notes |
| [API & Service Contracts](api-service-contracts.md) | Inventory of all 5 HTTP endpoints exposed by HomeController, DTO definitions, communication patterns with Azure AI Search, and security posture assessment |
| [Data Architecture](data-architecture.md) | Azure AI Search index schema documentation for `nycjobs` and `zipcodes` indexes, data ownership boundaries, repository methods, and data classification/sensitivity analysis |
| [Configuration Inventory](configuration-inventory.md) | Comprehensive inventory of all configuration sources (`Web.config`, `App.config`, XDT transforms), properties, secrets handling assessment, and framework/runtime version catalog |
| [Business Workflows](business-workflows.md) | End-to-end documentation of core business processes: job search with geo-distance filtering, auto-suggest, job detail lookup, and offline index initialisation; includes business rules and decision logic |
