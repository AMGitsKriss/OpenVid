using Database;
using Database.Models;
using Search.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Search.Filters
{
    [Filter(ParameterType.Tag)]
    public class TagFilter : IFilter
    {
        private Videos _repo;
        public TagFilter(Videos repo)
        {
            _repo = repo;
        }

        public List<Video> Filter(SearchParameter parameter)
        {
            return Tag(parameter.Value, parameter.InvertSearch);
        }

        public List<Video> Tag(string tag, bool invert)
        {
            var tagObject = _repo.GetAllTags().FirstOrDefault(x => x.Name.ToLower() == tag.ToLower());

            var result = _repo.VideosByTag().Where(x => x.TagId == tagObject.Id).Select(x => x.Video);
            if (invert)
            {
                var ids = result.Select(x => x.Id);
                result = _repo.VideosByTag().Where(x => !ids.Contains(x.VideoId)).Select(x => x.Video);
            }

            return result.ToList();
        }
    }
}
