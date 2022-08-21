using Database;
using Database.Models;
using Search.Filters;
using System.Collections.Generic;
using System.Linq;

namespace Search
{
    public class PaginatedSearch : SearchService
    {
        public PaginatedSearch(IVideoRepository repo, IEnumerable<IFilter> filters) : base(repo, filters) { }

        public List<Video> PaginatedQuery(string searchQuery, int pageNumber, out bool hasMore)
        {
            var results = Query(searchQuery).Skip((pageNumber - 1) * 48);
            hasMore = (results.Count() > 48);

            return results.Take(48).ToList();
        }
        //FileBaseUrl/thumbnail/@(video.Md5).jpg
    }
}
