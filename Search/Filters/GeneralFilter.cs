using Database;
using Database.Models;
using Search.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Search.Filters
{
    [Filter(ParameterType.General)]
    public class GeneralFilter : IFilter
    {
        private Videos _repo;
        public GeneralFilter(Videos repo)
        {
            _repo = repo;
        }

        public List<Video> Filter(SearchParameter parameter)
        {
            return FindByName(parameter.Value, parameter.InvertSearch);
        }

        public List<Video> FindByName(string tag, bool invert)
        {
            List<Video> result = _repo.GetAllVideos().Where(v => v.Name.Contains(tag) == !invert).ToList();

            return result;

        }
    }
}
