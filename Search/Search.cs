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
        private Videos _repo;
        private IEnumerable<IFilter> _filters;

        public Search(Videos repo, IEnumerable<IFilter> filters)
        {
            _repo = repo;
            _filters = filters;
        }
        public List<Video> Query(string searchQuery)
        {
            var parameters = Deconstruct(searchQuery);

            var results = new List<Video>();

            foreach (var item in parameters)
            {
                var filter = FilterAttribute.GetFilter(_filters, item.Type);
                results.AddRange(filter.Filter(item));
            }
            return results;
        }

        public List<SearchParameter> Deconstruct(string searchQuery)
        {
            searchQuery = searchQuery.ToLower().Trim();
            string[] splitQuery = searchQuery.Split(new char[] { ' ', '\n' }, 2);
            splitQuery = splitQuery.Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            List<SearchParameter> parameters = new List<SearchParameter>();
            foreach (var item in splitQuery)
            {
                string[] pair = item.Split(':', 2);
                var type = ParameterType.General;
                var value = pair[0];

                if (pair.Length == 2)
                {
                    type = (ParameterType)Enum.Parse(typeof(ParameterType), pair[0], true);
                    value = pair[1];
                }

                var p = new SearchParameter()
                {
                    Type = type,
                    Value = value
                };
                parameters.Add(p);
            }

            return parameters;
        }
    }

    public class SearchParameter
    {
        public ParameterType Type { get; set; }
        public string Value { get; set; }
        public string ConnectionString { get; set; }
    }

    public enum ParameterType
    {
        General,
        Tag,
        Meta,
        MinDuration,
        MaxDuration
    }
}
