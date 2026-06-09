using Microsoft.AspNetCore.Mvc;
using NYCJobsWeb.Models;
using System.Globalization;

namespace NYCJobsWeb.Controllers;

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

    public IActionResult Error()
    {
        return View();
    }

    [AcceptVerbs("GET", "POST")]
    public IActionResult Search(string q = "", string businessTitleFacet = "", string postingTypeFacet = "", string salaryRangeFacet = "",
        string sortType = "", double lat = 40.736224, double lon = -73.99251, int currentPage = 0, int zipCode = 10001,
        int maxDistance = 0)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                q = "*";
            }

            var maxDistanceLat = string.Empty;
            var maxDistanceLon = string.Empty;

            if (maxDistance > 0)
            {
                var zipResponse = _jobsSearch.SearchZip(zipCode.ToString());
                foreach (var result in zipResponse.GetResults())
                {
                    var doc = (dynamic)result.Document;
                    maxDistanceLat = Convert.ToString(doc["geo_location"].Latitude, CultureInfo.InvariantCulture);
                    maxDistanceLon = Convert.ToString(doc["geo_location"].Longitude, CultureInfo.InvariantCulture);
                }
            }

            var response = _jobsSearch.Search(q, businessTitleFacet, postingTypeFacet, salaryRangeFacet, sortType, lat, lon, currentPage, maxDistance, maxDistanceLat, maxDistanceLon);
            return Json(new NYCJob
            {
                Results = response.GetResults().ToList(),
                Facets = response.Facets,
                Count = Convert.ToInt32(response.TotalCount)
            });
        }
        catch
        {
            return Problem(statusCode: StatusCodes.Status500InternalServerError, detail: "Search service unavailable");
        }
    }

    [HttpGet]
    public IActionResult Suggest(string term, bool fuzzy = true)
    {
        try
        {
            var response = _jobsSearch.Suggest(term, fuzzy);
            var suggestions = new List<string>();
            foreach (var result in response.Results)
            {
                suggestions.Add(result.Text);
            }

            return Json(suggestions.Distinct().ToList());
        }
        catch
        {
            return Problem(statusCode: StatusCodes.Status500InternalServerError, detail: "Suggest service unavailable");
        }
    }

    [AcceptVerbs("GET", "POST")]
    public IActionResult LookUp(string id)
    {
        if (id != null)
        {
            try
            {
                var response = _jobsSearch.LookUp(id);
                return Json(new NYCJobLookup { Result = response });
            }
            catch
            {
                return Problem(statusCode: StatusCodes.Status500InternalServerError, detail: "Lookup service unavailable");
            }
        }

        return new EmptyResult();
    }
}
