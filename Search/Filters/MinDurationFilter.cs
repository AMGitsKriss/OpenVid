using Database;
using Database.Models;
using Search.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Search.Filters
{
    [Filter(ParameterType.MinDuration)]
    public class MinDurationFilter : IFilter
    {
        private IVideoRepository _repo;
        public MinDurationFilter(IVideoRepository repo)
        {
            _repo = repo;
        }

        public List<Video> Filter(SearchParameter parameter)
        {
            var duration = TimeSpan.Parse(parameter.Value);
            var results = _repo.GetViewableVideos().Where(x => x.Length >= duration).ToList();
            return results;
        }
    }
}
