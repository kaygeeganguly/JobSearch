using Azure.Search.Documents.Models;

namespace NYCJobsWeb.Models
{
    public class NYCJob
    {
        public IDictionary<string, IList<FacetResult>> Facets { get; set; }
        public IList<SearchResult<SearchDocument>> Results { get; set; }
        public int? Count { get; set; }
    }

    public class NYCJobLookup
    {
        public SearchDocument Result { get; set; }
    }
}
