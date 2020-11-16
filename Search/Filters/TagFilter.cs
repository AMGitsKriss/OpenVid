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
            return Tag(parameter.Value);
        }

        public List<Video> Tag(string tag)
        {
            var tagObject = _repo.GetAllTags().FirstOrDefault(x => x.Name.ToLower() == tag.ToLower());
            var result = _repo.VideosByTag().Where(x => x.TagId == tagObject.Id).Select(x=>x.Video).ToList();
            return result;
        }
    }
}
