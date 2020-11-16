using Database;
using Database.Models;
using Search.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Search.Filters
{
    [Filter(ParameterType.MaxDuration)]
    public class MaxDurationFilter : IFilter
    {
        private Videos _repo;
        public MaxDurationFilter(Videos repo)
        {
            _repo = repo;
        }

        public List<Video> Filter(SearchParameter parameter)
        {
            var duration = TimeSpan.Parse(parameter.Value);
            var results = _repo.GetAllVideos().Where(x => x.Length <= duration).ToList();
            return results;
        }
    }
}
