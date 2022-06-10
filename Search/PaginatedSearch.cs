using Database;
using Database.Models;
using Search.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Search
{
    public class PaginatedSearch : SearchService
    {
        public PaginatedSearch(Videos repo, IEnumerable<IFilter> filters) : base(repo, filters) { }

        public List<Video> PaginatedQuery(string searchQuery, int pageNumber, out bool hasMore)
        {
            var results = Query(searchQuery).Skip((pageNumber - 1) * 48);
            hasMore = (results.Count() > 48);

            return results.Take(48).ToList();
        }
    }
}
