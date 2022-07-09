using Database;
using Database.Models;
using Search.Attributes;
using Search.Filters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Search
{
    public class SearchService
    {
        internal Videos _repo;
        private IEnumerable<IFilter> _filters;

        public SearchService(Videos repo, IEnumerable<IFilter> filters)
        {
            _repo = repo;
            _filters = filters;
        }
        public List<Video> Query(string searchQuery)
        {
            var parameters = MapSearchQueryToParameters(searchQuery);

            var order = parameters.FirstOrDefault(x => x.Type == ParameterType.Order);
            parameters.Remove(order);

            var results = new List<Video>();
            if (parameters.Any())
            {
                foreach (var item in parameters)
                {
                    var filter = FilterAttribute.GetFilter(_filters, item.Type); // TODO - Fail gracefully if a filter is missing

                    var paramResult = filter.Filter(item);

                    if (!results.Any())
                        results.AddRange(paramResult);
                    else
                        results = results.Intersect(paramResult, new Comparer()).ToList();
                }
            }
            else
            {
                results.AddRange(_repo.GetAllVideos());
            }

            if (order?.Value == "random")
                results = results.OrderBy(x => Guid.NewGuid()).ToList();
            else if (order?.Value == "size")
                results = results.OrderByDescending(x => x.Size).ToList();
            else if (order?.Value == "size_asc")
                results = results.OrderBy(x => x.Size).ToList();
            else if (order?.Value == "duration")
                results = results.OrderByDescending(x => x.Length).ToList();
            else if (order?.Value == "duration_asc")
                results = results.OrderBy(x => x.Length).ToList();
            else if (order?.Value == "name")
                results = results.OrderBy(x => x.Name).ToList();
            else if (order?.Value == "name_za")
                results = results.OrderByDescending(x => x.Name).ToList();
            else if (order?.Value == "id_asc")
                results = results.OrderBy(x => x.Id).ToList();
            else
                results = results.OrderByDescending(x => x.Id).ToList();

            return results;
        }

        public List<SearchParameter> MapSearchQueryToParameters(string searchQuery)
        {
            searchQuery = searchQuery.ToLower().Trim();
            string[] splitQuery = searchQuery.Split(new char[] { ' ', '\n' });
            splitQuery = splitQuery.Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            List<SearchParameter> parameters = new List<SearchParameter>();
            foreach (var item in splitQuery)
            {
                SearchParameter param;

                if (item.Contains(":"))
                    param = TagSearch(item);
                else
                    param = StringSearch(item);

                if (param != null)
                    parameters.Add(param);
            }

            return parameters;
        }

        private SearchParameter TagSearch(string searchValue)
        {
            string[] pair = searchValue.Split(':', 2);
            var value = pair[0];
            var invert = pair[0].IndexOf('-') == 0 ? true : false;

            var isSuccess = Enum.TryParse(typeof(ParameterType), pair[0].Trim('-'), true, out var type);
            if (pair.Length == 2)
            {
                value = pair[1];
            }

            if (isSuccess)
            {
                return new SearchParameter()
                {
                    Type = (ParameterType)type,
                    Value = value,
                    InvertSearch = invert
                };
            }
            return null;
        }

        private SearchParameter StringSearch(string searchValue)
        {
            var invert = searchValue.IndexOf('-') == 0 ? true : false;

            var p = new SearchParameter()
            {
                Type = ParameterType.General,
                Value = searchValue.Trim('-'),
                InvertSearch = invert
            };
            return p;
        }
    }
}
