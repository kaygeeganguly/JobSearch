using Microsoft.AspNetCore.Mvc;
using NYCJobsWeb.Models;

namespace NYCJobsWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly JobsSearch _jobsSearch;

        public HomeController(JobsSearch jobsSearch)
        {
            _jobsSearch = jobsSearch;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult JobDetails()
        {
            return View();
        }

        public IActionResult Search(
            string q = "",
            string businessTitleFacet = "",
            string postingTypeFacet = "",
            string salaryRangeFacet = "",
            string sortType = "",
            double lat = 40.736224,
            double lon = -73.99251,
            int currentPage = 0,
            int zipCode = 10001,
            int maxDistance = 0)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                q = "*";
            }

            string maxDistanceLat = string.Empty;
            string maxDistanceLon = string.Empty;

            if (maxDistance > 0)
            {
                var zipResponse = _jobsSearch.SearchZip(zipCode.ToString());
                if (zipResponse != null)
                {
                    foreach (var result in zipResponse.GetResults())
                    {
                        dynamic doc = result.Document;
                        maxDistanceLat = Convert.ToString(doc["geo_location"].Latitude, System.Globalization.CultureInfo.InvariantCulture);
                        maxDistanceLon = Convert.ToString(doc["geo_location"].Longitude, System.Globalization.CultureInfo.InvariantCulture);
                    }
                }
            }

            var response = _jobsSearch.Search(q, businessTitleFacet, postingTypeFacet, salaryRangeFacet, sortType, lat, lon, currentPage, maxDistance, maxDistanceLat, maxDistanceLon);
            var results = response == null ? new List<Azure.Search.Documents.Models.SearchResult<Azure.Search.Documents.Models.SearchDocument>>() : response.GetResults().ToList();
            var facets = response?.Facets ?? new Dictionary<string, IList<Azure.Search.Documents.Models.FacetResult>>();
            var totalCount = response?.TotalCount ?? 0;

            return Json(new NYCJob
            {
                Results = results,
                Facets = facets,
                Count = Convert.ToInt32(totalCount)
            });
        }

        [HttpGet]
        public IActionResult Suggest(string term, bool fuzzy = true)
        {
            var response = _jobsSearch.Suggest(term, fuzzy);
            List<string> suggestions = new List<string>();
            if (response != null)
            {
                foreach (var result in response.Results)
                {
                    suggestions.Add(result.Text);
                }
            }

            return Json(suggestions.Distinct().ToList());
        }

        public IActionResult LookUp(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            var response = _jobsSearch.LookUp(id);
            return Json(new NYCJobLookup { Result = response });
        }
    }
}
