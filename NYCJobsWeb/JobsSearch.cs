using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using System.Globalization;

namespace NYCJobsWeb
{
    public class JobsSearch
    {
        private const string IndexName = "nycjobs";
        private const string ZipCodesIndexName = "zipcodes";

        private readonly SearchClient _indexClient;
        private readonly SearchClient _indexZipClient;

        public JobsSearch(IConfiguration configuration)
        {
            var endpoint = configuration["Search:Endpoint"];
            var serviceName = configuration["Search:ServiceName"];
            var apiKey = configuration["Search:ApiKey"];

            if (string.IsNullOrWhiteSpace(endpoint) && !string.IsNullOrWhiteSpace(serviceName))
            {
                endpoint = $"https://{serviceName}.search.windows.net";
            }

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new InvalidOperationException("Search endpoint configuration is missing.");
            }

            var credential = new AzureKeyCredential(apiKey ?? string.Empty);
            var searchIndexClient = new SearchIndexClient(new Uri(endpoint), credential);
            _indexClient = searchIndexClient.GetSearchClient(IndexName);
            _indexZipClient = searchIndexClient.GetSearchClient(ZipCodesIndexName);
        }

        public SearchResults<SearchDocument> Search(
            string searchText,
            string businessTitleFacet,
            string postingTypeFacet,
            string salaryRangeFacet,
            string sortType,
            double lat,
            double lon,
            int currentPage,
            int maxDistance,
            string maxDistanceLat,
            string maxDistanceLon)
        {
            try
            {
                SearchOptions searchOptions = new SearchOptions
                {
                    SearchMode = SearchMode.Any,
                    Size = 10,
                    Skip = Math.Max(currentPage - 1, 0),
                    IncludeTotalCount = true,
                    HighlightPreTag = "<b>",
                    HighlightPostTag = "</b>"
                };

                AddList(searchOptions.Select, new List<string>
                {
                    "id", "agency", "posting_type", "num_of_positions", "business_title",
                    "salary_range_from", "salary_range_to", "salary_frequency", "work_location", "job_description",
                    "posting_date", "geo_location", "tags"
                });
                AddList(searchOptions.Facets, new List<string>
                {
                    "business_title", "posting_type", "level", "salary_range_from,interval:50000"
                });
                searchOptions.HighlightFields.Add("job_description");

                if (sortType == "featured")
                {
                    searchOptions.ScoringProfile = "jobsScoringFeatured";
                    searchOptions.ScoringParameters.Add("featuredParam--featured");
                    searchOptions.ScoringParameters.Add($"mapCenterParam--geography'POINT({lon.ToString(CultureInfo.InvariantCulture)} {lat.ToString(CultureInfo.InvariantCulture)})'");
                }
                else if (sortType == "salaryDesc")
                {
                    searchOptions.OrderBy.Add("salary_range_from desc");
                }
                else if (sortType == "salaryIncr")
                {
                    searchOptions.OrderBy.Add("salary_range_from");
                }
                else if (sortType == "mostRecent")
                {
                    searchOptions.OrderBy.Add("posting_date desc");
                }

                string filter = null;
                if (!string.IsNullOrEmpty(businessTitleFacet))
                {
                    filter = "business_title eq '" + businessTitleFacet.Replace("'", "''") + "'";
                }

                if (!string.IsNullOrEmpty(postingTypeFacet))
                {
                    if (filter != null)
                    {
                        filter += " and ";
                    }

                    filter += "posting_type eq '" + postingTypeFacet.Replace("'", "''") + "'";
                }

                if (!string.IsNullOrEmpty(salaryRangeFacet))
                {
                    if (filter != null)
                    {
                        filter += " and ";
                    }

                    filter += "salary_range_from ge " + salaryRangeFacet + " and salary_range_from lt " + (Convert.ToInt32(salaryRangeFacet, CultureInfo.InvariantCulture) + 50000).ToString(CultureInfo.InvariantCulture);
                }

                if (maxDistance > 0)
                {
                    if (filter != null)
                    {
                        filter += " and ";
                    }

                    filter += $"geo.distance(geo_location, geography'POINT({maxDistanceLon} {maxDistanceLat})') le {maxDistance.ToString(CultureInfo.InvariantCulture)}";
                }

                searchOptions.Filter = filter;

                return _indexClient.Search<SearchDocument>(searchText, searchOptions).Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error querying index: {0}\r\n", ex.Message);
                return null;
            }
        }

        public SearchResults<SearchDocument> SearchZip(string zipCode)
        {
            try
            {
                SearchOptions searchOptions = new SearchOptions
                {
                    SearchMode = SearchMode.All,
                    Size = 1,
                };

                return _indexZipClient.Search<SearchDocument>(zipCode, searchOptions).Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error querying index: {0}\r\n", ex.Message);
                return null;
            }
        }

        public SuggestResults<SearchDocument> Suggest(string searchText, bool fuzzy)
        {
            try
            {
                SuggestOptions suggestOptions = new SuggestOptions
                {
                    UseFuzzyMatching = fuzzy,
                    Size = 8
                };

                return _indexClient.Suggest<SearchDocument>(searchText, "sg", suggestOptions).Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error querying index: {0}\r\n", ex.Message);
                return null;
            }
        }

        public SearchDocument LookUp(string id)
        {
            try
            {
                return _indexClient.GetDocument<SearchDocument>(id).Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error querying index: {0}\r\n", ex.Message);
                return null;
            }
        }

        private static void AddList(IList<string> destination, IEnumerable<string> source)
        {
            foreach (string element in source)
            {
                destination.Add(element);
            }
        }
    }
}
