using Database;
using Database.Models;
using Search.Attributes;
using Search.Filters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Search
{
    public class Search
    {
        internal Videos _repo;
        private IEnumerable<IFilter> _filters;

        public Search(Videos repo, IEnumerable<IFilter> filters)
        {
            _repo = repo;
            _filters = filters;
        }
        public List<Video> Query(string searchQuery)
        {
            var parameters = Deconstruct(searchQuery);

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

            return results;
        }

        public List<SearchParameter> Deconstruct(string searchQuery)
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

                parameters.Add(param);


            }

            return parameters;
        }

        private SearchParameter TagSearch(string searchValue)
        {
            string[] pair = searchValue.Split(':', 2);
            var type = ParameterType.Tag;
            var value = pair[0];
            var invert = false;

            if (pair.Length == 2)
            {
                invert = pair[0].IndexOf('-') == 0 ? true : false;
                pair[0] = pair[0].Trim('-');
                type = (ParameterType)Enum.Parse(typeof(ParameterType), pair[0], true);
                value = pair[1];
            }

            var p = new SearchParameter()
            {
                Type = type,
                Value = value,
                InvertSearch = invert
            };
            return p;
        }

        private SearchParameter StringSearch(string searchValue)
        {
            var invert = searchValue.IndexOf('-') == 0 ? true : false;

            var p = new SearchParameter()
            {
                Type = ParameterType.General,
                Value = searchValue,
                InvertSearch = invert
            };
            return p;
        }
    }

    public class SearchParameter
    {
        public ParameterType Type { get; set; }
        public string Value { get; set; }
        public bool InvertSearch { get; set; }
    }

    public enum ParameterType
    {
        General,
        Tag,
        Meta,
        MinDuration,
        MaxDuration,
        Order,
        Extension,
        Rating
    }
}
