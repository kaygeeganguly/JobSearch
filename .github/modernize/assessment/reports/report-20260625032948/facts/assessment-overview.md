# Assessment Overview

This document is the navigation entry point for the supplementary analysis documents generated as part of the NYC Jobs application assessment. Each document below provides a detailed view of a specific aspect of the application architecture and design.

## Supplementary Documents

| Document | Description |
|----------|-------------|
| [Architecture Diagram](architecture-diagram.md) | Two-layer architecture visualization: high-level application architecture (technology stack, data storage, external services) and detailed component relationship diagram grouped by architectural layer. |
| [Dependency Map](dependency-map.md) | Visual map of all external NuGet package dependencies grouped by functional category (web frameworks, search/Azure AI, geocoding, serialization, runtime polyfills), with version/compatibility risk analysis and notable observations. |
| [API & Service Contracts](api-service-contracts.md) | Catalog of all HTTP endpoints, request/response types, communication patterns, DTOs, security posture, and a Mermaid sequence diagram of the primary request flow across services and external APIs. |
| [Data Architecture](data-architecture.md) | Azure AI Search index schemas, field definitions, data ownership boundaries, key query methods (search, suggest, lookup, geo-filter), caching strategy, and data classification/sensitivity analysis. |
| [Configuration Inventory](configuration-inventory.md) | Comprehensive inventory of all configuration sources (`Web.config`, `App.config`), build profiles, runtime profiles, property key-value pairs, secrets management approach, startup dependency chain, and framework/runtime version catalog. |
| [Business Workflows](business-workflows.md) | End-to-end documentation of core business processes (full-text search, geo-proximity search, autocomplete, job detail lookup, index seeding), domain entity descriptions, business rules, decision logic, and a Mermaid sequence diagram of the primary workflow. |
