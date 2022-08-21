using Database;
using Database.Models;
using Search.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Search.Filters
{
    [Filter(ParameterType.Extension)]
    public class ExtensionFilter : IFilter
    {
        private Videos _repo;
        public ExtensionFilter(Videos repo)
        {
            _repo = repo;
        }

        public List<Video> Filter(SearchParameter parameter)
        {
            return Extension(parameter.Value, parameter.InvertSearch);
        }

        public List<Video> Extension(string extension, bool invert)
        {
            extension = extension.ToLower();

            List<Video> result;
            if (invert)
            {
                result = _repo.GetAllVideos().Where(x => !(x.VideoSource.Any(s => s.Extension.ToLower() == extension))).ToList();
            }
            else
            {
                result = _repo.GetAllVideos().Where(x => x.VideoSource.Any(s => s.Extension.ToLower() == extension)).ToList();
            }
            return result.ToList();

        }
    }
}
