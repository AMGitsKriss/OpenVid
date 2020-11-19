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
    [Filter(ParameterType.Meta)]
    public class MetaFilter : IFilter
    {
        private Videos _repo;
        public MetaFilter(Videos repo)
        {
            _repo = repo;
        }

        public List<Video> Filter(SearchParameter parameter)
        {
            Type thisType = GetType();
            MethodInfo theMethod = thisType.GetMethod(parameter.Value, BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.Instance);
            return (List<Video>)theMethod.Invoke(this, null);
        }

        private List<Video> Untagged()
        {
            var result = _repo.GetAllVideos().Where(x => x.VideoTag.Count() == 0).ToList();
            return result;
        }

        private List<Video> Tagged()
        {
            var result = _repo.GetAllVideos().Where(x => x.VideoTag.Count() != 0).ToList();
            return result;
        }

        private List<Video> Thumbnail()
        {
            throw new NotImplementedException();
        }

        private List<Video> NoThumbnail()
        {
            throw new NotImplementedException();
        }
    }
}
