using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using System.Configuration;

namespace NYCJobsWeb;

public class JobsSearch
{
    private static SearchClient _indexClient;
    private static string IndexName = "nycjobs";
    private static SearchClient _indexZipClient;
    private static string IndexZipCodes = "zipcodes";

    public static string errorMessage;

    static JobsSearch()
    {
        try
        {
            string searchendpoint = System.Configuration.ConfigurationManager.AppSettings["Searchendpoint"];
            string apiKey = System.Configuration.ConfigurationManager.AppSettings["SearchServiceApiKey"];

            _indexClient = new SearchIndexClient(new Uri(searchendpoint), new AzureKeyCredential(apiKey)).GetSearchClient(IndexName);
            _indexZipClient = new SearchIndexClient(new Uri(searchendpoint), new AzureKeyCredential(apiKey)).GetSearchClient(IndexZipCodes);
        }
        catch (Exception e)
        {
            errorMessage = e.Message.ToString();
        }
    }

    public SearchResults<SearchDocument> Search(string searchText, string businessTitleFacet, string postingTypeFacet, string salaryRangeFacet,
        string sortType, double lat, double lon, int currentPage, int maxDistance, string maxDistanceLat, string maxDistanceLon)
    {
        try
        {
            SearchOptions sp = new SearchOptions()
            {
                SearchMode = SearchMode.Any,
                Size = 10,
                Skip = currentPage - 1,
                IncludeTotalCount = true,
                HighlightPreTag = "<b>",
                HighlightPostTag = "</b>",
            };
            List<string> select = new List<string>() { "id", "agency", "posting_type", "num_of_positions", "business_title",
                    "salary_range_from", "salary_range_to", "salary_frequency", "work_location", "job_description",
                    "posting_date", "geo_location", "tags"};
            List<string> facets = new List<string>() { "business_title", "posting_type", "level", "salary_range_from,interval:50000" };
            AddList(sp.Select, select);
            AddList(sp.Facets, facets);
            sp.HighlightFields.Add("job_description");

            if (sortType == "featured")
            {
                sp.ScoringProfile = "jobsScoringFeatured";
                sp.ScoringParameters.Add("featuredParam--featured");
                sp.ScoringParameters.Add("mapCenterParam--" + lon + "," + lat);
            }
            else if (sortType == "salaryDesc")
            {
                sp.OrderBy.Add("salary_range_from desc");
            }
            else if (sortType == "salaryIncr")
            {
                sp.OrderBy.Add("salary_range_from");
            }
            else if (sortType == "mostRecent")
            {
                sp.OrderBy.Add("posting_date desc");
            }

            string filter = null;
            if (businessTitleFacet != "")
                filter = "business_title eq '" + businessTitleFacet + "'";
            if (postingTypeFacet != "")
            {
                if (filter != null)
                    filter += " and ";
                filter += "posting_type eq '" + postingTypeFacet + "'";
            }
            if (salaryRangeFacet != "")
            {
                if (filter != null)
                    filter += " and ";
                filter += "salary_range_from ge " + salaryRangeFacet + " and salary_range_from lt " + (Convert.ToInt32(salaryRangeFacet) + 50000).ToString();
            }

            if (maxDistance > 0)
            {
                if (filter != null)
                    filter += " and ";
                filter += "geo.distance(geo_location, geography'POINT(" + maxDistanceLon + " " + maxDistanceLat + ")') le " + maxDistance.ToString();
            }

            sp.Filter = filter;

            return _indexClient.Search<SearchDocument>(searchText, sp);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error querying index: {0}", ex.Message.ToString());
        }
        return null;
    }

    public SearchResults<SearchDocument> SearchZip(string zipCode)
    {
        try
        {
            SearchOptions sp = new SearchOptions()
            {
                SearchMode = SearchMode.All,
                Size = 1,
            };
            return _indexZipClient.Search<SearchDocument>(zipCode, sp);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error querying index: {0}", ex.Message.ToString());
        }
        return null;
    }

    public SuggestResults<SearchDocument> Suggest(string searchText, bool fuzzy)
    {
        try
        {
            SuggestOptions sp = new SuggestOptions()
            {
                UseFuzzyMatching = fuzzy,
                Size = 8
            };

            return _indexClient.Suggest<SearchDocument>(searchText, "sg", sp);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error querying index: {0}", ex.Message.ToString());
        }
        return null;
    }

    public SearchDocument LookUp(string id)
    {
        try
        {
            return _indexClient.GetDocument<SearchDocument>(id);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error querying index: {0}", ex.Message.ToString());
        }
        return null;
    }

    public void AddList(IList<string> list1, List<string> list2)
    {
        foreach (string element in list2)
        {
            list1.Add(element);
        }
    }
}
