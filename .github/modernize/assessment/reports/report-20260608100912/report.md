# DataLoader

## Summary

| Metric | Value |
|--------|-------|
| Total Issues | 20 |
| Mandatory Blockers | 8 |
| Potential Issues | 7 |

## Component Information

| Property | Value |
|----------|-------|
| Language | C# |
| Frameworks | .NETFramework,Version=v4.5 |
| Build tools | MSBuild |

## Cloud Readiness Issues

| Issue Name | Criticality | Story Points | Occurrences |
|------------|-------------|--------------|-------------|
| Access to external resources via HTTP is detected | Potential | 3 | [3](#Access_to_external_resources_via_HTTP_is_detected) |
| Local or network IO operations detected | Potential | 3 | [3](#Local_or_network_IO_operations_detected) |
| Old .NET Framework dependency detected | Potential | 3 | [1](#Old_NET_Framework_dependency_detected) |
| Hardcoded URLs detected | Potential | 1 | [1](#Hardcoded_URLs_detected) |
| Synchronous API usage detected | Optional | 1 | [2](#Synchronous_API_usage_detected) |
| Connection strings without configuration builders detected | Optional | 3 | [1](#Connection_strings_without_configuration_builders_detected) |
| Static content detected | Optional | 3 | [1](#Static_content_detected) |

### Issue Details

<details id="Access_to_external_resources_via_HTTP_is_detected">
<summary><b>Access to external resources via HTTP is detected</b> — affected files</summary>

- `DataLoader\AzureSearchHelper.cs (line 51)`
- `DataLoader\Program.cs (line 21)`
- `DataLoader\Program.cs (line 13)`

</details>

<details id="Local_or_network_IO_operations_detected">
<summary><b>Local or network IO operations detected</b> — affected files</summary>

- `DataLoader\Program.cs (line 96)`
- `DataLoader\Program.cs (line 77)`
- `DataLoader\Program.cs (line 99)`

</details>

<details id="Old_NET_Framework_dependency_detected">
<summary><b>Old .NET Framework dependency detected</b> — affected files</summary>

- `DataLoader\DataLoader.csproj`

</details>

<details id="Hardcoded_URLs_detected">
<summary><b>Hardcoded URLs detected</b> — affected files</summary>

- `DataLoader\Program.cs (line 20)`

</details>

<details id="Synchronous_API_usage_detected">
<summary><b>Synchronous API usage detected</b> — affected files</summary>

- `DataLoader\AzureSearchHelper.cs (line 64)`
- `DataLoader\AzureSearchHelper.cs (line 71)`

</details>

<details id="Connection_strings_without_configuration_builders_detected">
<summary><b>Connection strings without configuration builders detected</b> — affected files</summary>

- `DataLoader\App.config`

</details>

<details id="Static_content_detected">
<summary><b>Static content detected</b> — affected files</summary>

- `D:\a\JobSearch\JobSearch\NYCJobsWeb\NYCJobsWeb.csproj`

</details>

## DotNET Upgrade Issues [View Details](scenarios/dotnet-version-upgrade/assessment.md)

| Issue Category | Criticality | Story Points | Occurrences |
|----------------|-------------|--------------|-------------|
| NuGet package functionality is included with framework reference | Mandatory | 1 | [9](#NuGet_package_functionality_is_included_with_framework_reference) |
| Binary incompatible for selected .NET version | Mandatory | 1 | [4](#Binary_incompatible_for_selected_NET_version) |
| Routes registration via RouteCollection is not supported in .NET Core and needs to be converted to the route mappings on the application object | Mandatory | 1 | [2](#Routes_registration_via_RouteCollection_is_not_supported_in_NET_Core_and_needs_to_be_converted_to_the_route_mappings_on_the_application_object) |
| Project file needs to be converted to SDK-style | Mandatory | 1 | [1](#Project_file_needs_to_be_converted_to_SDK-style) |
| Project's target framework(s) needs to be changed | Mandatory | 1 | [1](#Project_s_target_framework_s_needs_to_be_changed) |
| Convert application initialization code from Global.asax.cs to .NET Core and clean up Global.asax.cs | Mandatory | 1 | [1](#Convert_application_initialization_code_from_Global_asax_cs_to_NET_Core_and_clean_up_Global_asax_cs) |
| Legacy Configuration System | Mandatory | 2 | 0 |
| ASP.NET Framework (System.Web) | Mandatory | 4 | 0 |
| Source incompatible for selected .NET version | Potential | 1 | [10](#Source_incompatible_for_selected_NET_version) |
| Behavioral change in selected .NET version | Potential | 1 | [8](#Behavioral_change_in_selected_NET_version) |
| NuGet package upgrade is recommended | Potential | 1 | [6](#NuGet_package_upgrade_is_recommended) |
| NuGet package contains security vulnerability | Optional | 1 | [4](#NuGet_package_contains_security_vulnerability) |
| NuGet package is deprecated | Optional | 1 | [3](#NuGet_package_is_deprecated) |

### Issue Details

<details id="NuGet_package_functionality_is_included_with_framework_reference">
<summary><b>NuGet package functionality is included with framework reference</b> — affected files</summary>

- `NYCJobsWeb\NYCJobsWeb.csproj`

</details>

<details id="Binary_incompatible_for_selected_NET_version">
<summary><b>Binary incompatible for selected .NET version</b> — affected files</summary>

- `NYCJobsWeb\Global.asax.cs (line 14, col 12)`
- `NYCJobsWeb\App_Start\RouteConfig.cs (line 11, col 8)`

</details>

<details id="Routes_registration_via_RouteCollection_is_not_supported_in_NET_Core_and_needs_to_be_converted_to_the_route_mappings_on_the_application_object">
<summary><b>Routes registration via RouteCollection is not supported in .NET Core and needs to be converted to the route mappings on the application object</b> — affected files</summary>

- `NYCJobsWeb\Global.asax.cs`
- `NYCJobsWeb\App_Start\RouteConfig.cs`

</details>

<details id="Project_file_needs_to_be_converted_to_SDK-style">
<summary><b>Project file needs to be converted to SDK-style</b> — affected files</summary>

- `NYCJobsWeb\NYCJobsWeb.csproj`

</details>

<details id="Project_s_target_framework_s_needs_to_be_changed">
<summary><b>Project's target framework(s) needs to be changed</b> — affected files</summary>

- `NYCJobsWeb\NYCJobsWeb.csproj`

</details>

<details id="Convert_application_initialization_code_from_Global_asax_cs_to_NET_Core_and_clean_up_Global_asax_cs">
<summary><b>Convert application initialization code from Global.asax.cs to .NET Core and clean up Global.asax.cs</b> — affected files</summary>

- `NYCJobsWeb\Global.asax.cs`

</details>

<details id="Source_incompatible_for_selected_NET_version">
<summary><b>Source incompatible for selected .NET version</b> — affected files</summary>

- `NYCJobsWeb\JobsSearch.cs (line 28, col 16)`
- `NYCJobsWeb\JobsSearch.cs (line 27, col 16)`
- `NYCJobsWeb\Global.asax.cs (line 9, col 45)`

</details>

<details id="Behavioral_change_in_selected_NET_version">
<summary><b>Behavioral change in selected .NET version</b> — affected files</summary>

- `NYCJobsWeb\JobsSearch.cs (line 32, col 16)`
- `NYCJobsWeb\JobsSearch.cs (line 31, col 16)`

</details>

<details id="NuGet_package_upgrade_is_recommended">
<summary><b>NuGet package upgrade is recommended</b> — affected files</summary>

- `NYCJobsWeb\NYCJobsWeb.csproj`

</details>

<details id="NuGet_package_contains_security_vulnerability">
<summary><b>NuGet package contains security vulnerability</b> — affected files</summary>

- `NYCJobsWeb\NYCJobsWeb.csproj`

</details>

<details id="NuGet_package_is_deprecated">
<summary><b>NuGet package is deprecated</b> — affected files</summary>

- `NYCJobsWeb\NYCJobsWeb.csproj`

</details>

---

## Codebase Insights

> **Note:** These documents are generated by AI and may contain inaccuracies or incomplete information. Please review carefully.

1. **[Architecture Diagram](facts/architecture-diagram.md)** — Understand the big picture: system layers and component relationships
2. **[Dependency Map](facts/dependency-map.md)** — Know what the project depends on and where the risks are
3. **[API & Service Contracts](facts/api-service-contracts.md)** — See how services communicate and what contracts they expose
4. **[Data Architecture](facts/data-architecture.md)** — Explore data models, storage, and data flow patterns
5. **[Configuration Inventory](facts/configuration-inventory.md)** — Review how the application is configured across environments
6. **[Business Workflows](facts/business-workflows.md)** — Trace end-to-end business processes and domain logic

[Share feedback](https://aka.ms/ghcp-appmod/feedback)
