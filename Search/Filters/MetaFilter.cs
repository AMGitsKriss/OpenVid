using Database;
using Database.Models;
using Search.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Search.Filters
{
    [Filter(ParameterType.Meta)]
    public class MetaFilter : IFilter
    {
        private IVideoRepository _repo;
        public MetaFilter(IVideoRepository repo)
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
            var result = _repo.GetViewableVideos().Where(x => x.VideoTag.Count() == 0).ToList();
            return result;
        }

        private List<Video> Tagged()
        {
            var result = _repo.GetViewableVideos().Where(x => x.VideoTag.Count() != 0).ToList();
            return result;
        }

        private List<Video> Deleted()
        {
            var result = _repo.GetSoftDeletedVideos().ToList();
            return result;
        }

        private List<Video> SameLength()
        {
            var result = _repo.GetViewableVideos().ToList().GroupBy(m => m, new DurationComparer())
                               .Where(a => a.Count() > 1)
                               .SelectMany(a => a.ToList());
            return result.ToList();
        }

        public class DurationComparer : IEqualityComparer<Video>
        {
            public bool Equals(Video x, Video y)
            {
                int secondsVariance = 3;

                return Math.Abs(x.Length.TotalSeconds - y.Length.TotalSeconds) <= secondsVariance;
            }

            public int GetHashCode([DisallowNull] Video obj)
            {
                return 1;
            }
        }
    }
}
