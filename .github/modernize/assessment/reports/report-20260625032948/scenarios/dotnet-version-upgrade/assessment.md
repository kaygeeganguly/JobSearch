# Projects and dependencies analysis

This document provides a comprehensive overview of the projects and their dependencies in the context of upgrading to .NETCoreApp,Version=v10.0.

## Table of Contents

- [Executive Summary](#executive-Summary)
  - [Highlevel Metrics](#highlevel-metrics)
  - [Projects Compatibility](#projects-compatibility)
  - [Package Compatibility](#package-compatibility)
  - [API Compatibility](#api-compatibility)
  - [Binding Redirect Configuration](#binding-redirect-configuration)
- [Aggregate NuGet packages details](#aggregate-nuget-packages-details)
- [Top API Migration Challenges](#top-api-migration-challenges)
  - [Technologies and Features](#technologies-and-features)
  - [Most Frequent API Issues](#most-frequent-api-issues)
- [Projects Relationship Graph](#projects-relationship-graph)
- [Project Details](#project-details)

  - [NYCJobsWeb/NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj)


## Executive Summary

### Highlevel Metrics

| Metric | Count | Status |
| :--- | :---: | :--- |
| Total Projects | 1 | All require upgrade |
| Total NuGet Packages | 24 | 9 need upgrade |
| Total Code Files | 10 |  |
| Total Code Files with Incidents | 2 |  |
| Total Lines of Code | 197 |  |
| Total Number of Issues | 25 |  |
| Estimated LOC to modify | 0+ | at least 0.0% of codebase |

### Projects Compatibility

| Project | Target Framework | Difficulty | Package Issues | API Issues | Binding Issues | Est. LOC Impact | Description |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :--- |
| [NYCJobsWeb/NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | net472 | 🟢 Low | 22 | 0 | 0 |  | ClassicClassLibrary, Sdk Style = False |

### Package Compatibility

| Status | Count | Percentage |
| :--- | :---: | :---: |
| ✅ Compatible | 15 | 62.5% |
| ⚠️ Incompatible | 1 | 4.2% |
| 🔄 Upgrade Recommended | 8 | 33.3% |
| ***Total NuGet Packages*** | ***24*** | ***100%*** |

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 0 |  |
| ***Total APIs Analyzed*** | ***0*** |  |

## Aggregate NuGet packages details

| Package | Current Version | Suggested Version | Projects | Description |
| :--- | :---: | :---: | :--- | :--- |
| Azure.Core | 1.4.1 |  | [NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | ✅Compatible |
| Azure.Search.Documents | 11.1.1 |  | [NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | ✅Compatible |
| BingGeocodingHelper | 1.1 |  | [NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | ✅Compatible |
| bootstrap | 3.4.1 |  | [NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | ✅Compatible |
| jQuery | 3.1.1 | 3.7.1 | [NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | NuGet package contains security vulnerability |
| Microsoft.AspNet.Mvc | 5.2.2 |  | [NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | NuGet package functionality is included with framework reference |
| Microsoft.AspNet.Razor | 3.2.2 |  | [NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | NuGet package functionality is included with framework reference |
| Microsoft.AspNet.WebPages | 3.2.2 |  | [NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | NuGet package functionality is included with framework reference |
| Microsoft.Bcl.AsyncInterfaces | 1.0.0 | 10.0.9 | [NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | NuGet package upgrade is recommended |
| Microsoft.Rest.ClientRuntime | 2.3.20 | 2.3.24 | [NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | NuGet package contains security vulnerability |
| Microsoft.Rest.ClientRuntime.Azure | 3.3.18 |  | [NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | ⚠️NuGet package is deprecated |
| Microsoft.Spatial | 7.5.3 |  | [NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | ✅Compatible |
| Microsoft.Web.Infrastructure | 1.0.0.0 |  | [NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | NuGet package functionality is included with framework reference |
| Modernizr | 2.8.3 |  | [NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | ✅Compatible |
| Newtonsoft.Json | 10.0.3 | 13.0.4 | [NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | NuGet package upgrade is recommended |
| System.Buffers | 4.5.0 |  | [NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | NuGet package functionality is included with framework reference |
| System.Diagnostics.DiagnosticSource | 4.6.0 | 10.0.9 | [NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | NuGet package upgrade is recommended |
| System.Memory | 4.5.3 |  | [NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | NuGet package functionality is included with framework reference |
| System.Numerics.Vectors | 4.5.0 |  | [NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | NuGet package functionality is included with framework reference |
| System.Runtime.CompilerServices.Unsafe | 4.6.0 | 6.1.2 | [NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | NuGet package upgrade is recommended |
| System.Text.Encodings.Web | 4.6.0 | 10.0.9 | [NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | NuGet package upgrade is recommended |
| System.Text.Json | 4.6.0 | 10.0.9 | [NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | NuGet package upgrade is recommended |
| System.Threading.Tasks.Extensions | 4.5.2 |  | [NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | NuGet package functionality is included with framework reference |
| System.ValueTuple | 4.5.0 |  | [NYCJobsWeb.csproj](#nycjobswebnycjobswebcsproj) | NuGet package functionality is included with framework reference |

## Top API Migration Challenges

### Technologies and Features

| Technology | Issues | Percentage | Migration Path |
| :--- | :---: | :---: | :--- |

### Most Frequent API Issues

| API | Count | Percentage | Category |
| :--- | :---: | :---: | :--- |

## Projects Relationship Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart LR

```

## Project Details

