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

            var order = parameters.FirstOrDefault(x => x.Type == ParameterType.Order);
            parameters.Remove(order);

            var results = new List<Video>();

            foreach (var item in parameters)
            {
                var filter = FilterAttribute.GetFilter(_filters, item.Type);

                var paramResult = filter.Filter(item);

                if (!results.Any())
                    results.AddRange(paramResult);
                else
                    results = results.Intersect(paramResult, new Comparer()).ToList();
            }

            if (order?.Value == "random")
                results = results.OrderBy(x => Guid.NewGuid()).ToList();

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
                string[] pair = item.Split(':', 2);
                var type = ParameterType.General;
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
                parameters.Add(p);
            }

            return parameters;
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
        Order
    }
}
