# Core Business Workflows

The NYC Jobs application is a public job search portal that allows job seekers to discover, filter, and explore New York City government job postings using full-text search, faceted navigation, geo-distance filtering, and auto-suggestions.

## Domain Entities

| Entity | Service / Bounded Context | Description | Key Relationships |
|--------|--------------------------|-------------|-------------------|
| NycJobDocument | Job Search (NYCJobsWeb) | Represents a single NYC government job posting with title, agency, salary, location, description, and posting dates | None (self-contained search document) |
| ZipcodeDocument | Geo-location (NYCJobsWeb) | Maps a US zip code to geographic coordinates (lat/lon) for distance-based filtering | Used by the Search workflow to resolve a user-entered zip code before filtering job postings by distance |
| NYCJob | API Response (NYCJobsWeb) | Response aggregate wrapping a page of search results, facet aggregations, and total count | Composed from NycJobDocument search results |
| NYCJobLookup | API Response (NYCJobsWeb) | Response aggregate carrying a single fully-resolved job document | Composed from one NycJobDocument |

For field-level details, see `data-architecture.md`.

## Service-to-Domain Mapping

| Service | Domain Context | Owned Entities | External Dependencies |
|---------|---------------|----------------|----------------------|
| NYCJobsWeb | Job Search Portal | NYCJob, NYCJobLookup (read-only; source of truth is Azure AI Search) | Azure AI Search (`nycjobs` and `zipcodes` indices) |
| DataLoader | Index Management | NycJobDocument, ZipcodeDocument (write ownership) | Azure AI Search (admin API) |

The two services share the same Azure AI Search service but have non-overlapping roles: DataLoader holds write authority over the index content, while NYCJobsWeb exercises read-only access.

## Primary Workflows

### Workflow 1: Full-Text Job Search with Faceting and Geo-Distance Filtering

**Entry point:** User enters a search term and presses Enter (or accepts an autocomplete suggestion), triggering an AJAX call to `GET /Home/Search`.

**Steps:**

1. **Blank query normalization:** If the search text is empty or whitespace, it is replaced with `*` (match-all wildcard) so that browsing without a query returns all available postings.
2. **Geo-distance pre-lookup (conditional):** If the user has entered a `maxDistance` greater than 0, the controller calls `JobsSearch.SearchZip(zipCode)` to resolve the zip code to a latitude/longitude coordinate pair from the `zipcodes` index. If the zip code is not found, the distance filter is silently skipped.
3. **Search query construction:** `JobsSearch.Search(...)` builds a `SearchOptions` object with:
   - `SearchMode.Any` (match any term)
   - Page size of 10, with cursor-based skipping via `currentPage - 1`
   - Hit highlighting on `job_description` (bold tags `<b>` / `</b>`)
   - Facet aggregation on `business_title`, `posting_type`, `level`, and `salary_range_from` (with $50,000 intervals)
   - Field projection: returns only the 13 fields needed for the results list view
4. **Sorting decision (business rule):**
   - `featured` → uses `jobsScoringFeatured` scoring profile, which boosts by recency (posting_date freshness), featured tags, and geo-proximity to the map center
   - `salaryDesc` → `salary_range_from desc`
   - `salaryIncr` → `salary_range_from asc`
   - `mostRecent` → `posting_date desc`
5. **Filter construction (business rules):**
   - Active `businessTitleFacet` → OData filter `business_title eq '...'`
   - Active `postingTypeFacet` → OData filter `posting_type eq '...'`
   - Active `salaryRangeFacet` → OData filter `salary_range_from ge X and salary_range_from lt X+50000`
   - Active `maxDistance` → OData geo-distance filter `geo.distance(geo_location, ...) le N`
   - Filters are AND-combined
6. **Results returned:** Controller wraps results into `NYCJob` and returns JSON to the browser.
7. **Client-side rendering:** The browser renders results, updates facet panels, updates the map pins, and rebuilds the pagination controls.

### Workflow 2: Auto-Suggest as User Types

**Entry point:** User types 2 or more characters in the search box, triggering jQuery autocomplete which calls `GET /Home/Suggest`.

**Steps:**

1. Controller passes the partial term to `JobsSearch.Suggest(term, fuzzy)`.
2. Azure AI Search executes a suggester query (`sg`) against the `nycjobs` index on fields: `agency`, `posting_type`, `business_title`, `civil_service_title`, `work_location`, `division_work_unit`.
3. Up to 8 suggestions are returned; the controller deduplicates them (`Distinct()`) and returns the unique list.
4. jQuery UI autocomplete renders the list as a dropdown; selecting an item triggers a full `Search()` call.

### Workflow 3: Job Detail Lookup

**Entry point:** User clicks a job posting in search results, triggering an AJAX call to `GET /Home/LookUp?id={id}`.

**Steps:**

1. Controller validates that `id` is not null; returns null response if missing.
2. Calls `JobsSearch.LookUp(id)` which uses `SearchClient.GetDocument<SearchDocument>(id)` — a key-based point lookup against the `nycjobs` index.
3. Returns the full `NYCJobLookup` JSON to the browser for display in the job detail panel.

### Workflow 4: Index Initialization (DataLoader — Offline)

**Entry point:** Manual execution of the `DataLoader` console application.

**Steps:**

1. Reads `TargetSearchServiceName` and `TargetSearchServiceApiKey` from `App.config`.
2. For each index name (`zipcodes`, `nycjobs`):
   a. Deletes the index if it exists (HTTP DELETE).
   b. Creates the index from the corresponding `.schema` JSON file (HTTP POST to `/indexes`).
   c. Uploads all matching `{indexName}*.json` data files from `Schema_and_Data/` in batch (HTTP POST to `/indexes/{name}/docs/index`).

## Cross-Service Data Flows

The application performs a **two-step sequential lookup** within the same service for geo-distance searches:

1. **Zip code resolution:** NYCJobsWeb queries the `zipcodes` index to convert a user-supplied zip code into coordinates. This is a point lookup with `SearchMode.All` and size 1.
2. **Geo-filtered job search:** The resolved coordinates are embedded into an OData `geo.distance()` filter in the job search query sent to the `nycjobs` index.

There is no cross-service HTTP call, no event-driven data flow, and no aggregation gateway. Both lookups are executed synchronously within a single HTTP request. If the zipcodes lookup fails (zip not found or exception), the geo-distance filter is omitted and the search proceeds without distance filtering — a silent degradation.

## Business Workflow Sequence

```mermaid
sequenceDiagram
    participant User as "Job Seeker (Browser)"
    participant Home as "HomeController"
    participant Jobs as "JobsSearch"
    participant ZipIdx as "zipcodes index"
    participant JobIdx as "nycjobs index"

    User->>Home: Type 3 chars in search box
    Home->>Jobs: Suggest("ana", fuzzy=true)
    Jobs->>JobIdx: Suggester query (sg) on title/agency/location fields
    JobIdx-->>Jobs: Up to 8 raw suggestions
    Jobs-->>Home: Deduplicated suggestion list
    Home-->>User: Autocomplete dropdown (up to 8 unique items)

    User->>Home: Select suggestion or press Enter (GET /Home/Search)
    Note over Home: Normalize blank query to wildcard (*)

    alt maxDistance > 0
        Home->>Jobs: SearchZip(zipCode)
        Jobs->>ZipIdx: Point lookup by zip code
        ZipIdx-->>Jobs: geo_location (lat, lon)
        Jobs-->>Home: Coordinates for distance filter
    else No distance filter
        Note over Home: Skip zip lookup; no geo filter applied
    end

    Home->>Jobs: Search(q, facets, sortType, lat, lon, page, maxDistance)
    Note over Jobs: Build OData filter (title + type + salary + geo)
    Note over Jobs: Select scoring profile based on sortType

    Jobs->>JobIdx: Full-text search with facets, filters, highlights
    JobIdx-->>Jobs: SearchResults (10 docs, facets, total count, highlights)
    Jobs-->>Home: SearchResults
    Home-->>User: JSON NYCJob (results, facets, count)

    User->>Home: Click job posting (GET /Home/LookUp?id=...)
    Home->>Jobs: LookUp(id)
    Jobs->>JobIdx: GetDocument by key
    JobIdx-->>Jobs: Full job document
    Jobs-->>Home: SearchDocument
    Home-->>User: JSON NYCJobLookup (full job detail)
```

## Business Rules & Decision Logic

**Validation Rules:**

- Blank search query is normalized to `*` (wildcard match-all) — no error is returned for empty searches.
- `LookUp` action returns `null` if `id` parameter is null — no error message is surfaced to the user.
- `maxDistance` must be greater than 0 for geo-filtering to activate; default is 0 (no distance filter).
- Auto-suggest requires a minimum of 2 characters before the first suggestion request is sent (client-side jQuery autocomplete `minLength: 2`).

**Decision Logic — Sort Mode:**

| sortType value | Behavior |
|---------------|----------|
| `featured` | Uses `jobsScoringFeatured` scoring profile: boosts by posting recency (P500D freshness window), featured tag match, and geo-proximity within 5 km of the map center |
| `salaryDesc` | Orders by `salary_range_from` descending |
| `salaryIncr` | Orders by `salary_range_from` ascending |
| `mostRecent` | Orders by `posting_date` descending |

**Facet Filtering (AND-combined, incremental):**

- Up to three facets can be active simultaneously: `businessTitleFacet`, `postingTypeFacet`, and `salaryRangeFacet`.
- Salary facet uses $50,000-wide intervals (`salary_range_from,interval:50000`).
- Removing a facet triggers a fresh search with that filter cleared; facets are client-side state variables in the browser.

**State Transitions:** The application is read-only (no entity lifecycle transitions for users). The only state-mutating workflow is index initialization by DataLoader (destroy → create → populate), which is a one-time administrative operation.

**Error Handling:**

- Azure AI Search SDK exceptions in `JobsSearch` are caught and logged to `Console.WriteLine`; all methods return `null` on exception. The controller does not check for null results, which may cause a NullReferenceException when the search service is unavailable.
- DataLoader catches top-level exceptions and prints a user-friendly message; individual index operations use `EnsureSuccessStatusCode()` to throw on HTTP error.

**Authorization:** None. All search, suggest, and lookup operations are publicly accessible without authentication.
