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

        private List<Video> Deleted()
        {
            var result = _repo.GetDeletedVideos().ToList();
            return result;
        }

        private List<Video> SameLength()
        {
            var result = _repo.GetAllVideos().ToList().GroupBy(m => m, new DurationComparer())
                               .Where(a => a.Count() > 1)
                               .SelectMany(a => a.ToList());
            return result.ToList();
        }

        private List<Video> SimilarName()
        {
            var result = _repo.GetAllVideos().ToList().GroupBy(m => m, new NameComparer())
                               .Where(a => a.Count() > 1)
                               .SelectMany(a => a.ToList());
            return result.ToList();
        }

        private List<Video> SimilarThumbnail()
        {
            throw new NotImplementedException();
        }

        private List<Video> Thumbnail()
        {
            throw new NotImplementedException();
        }

        private List<Video> NoThumbnail()
        {
            throw new NotImplementedException();
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
        public class NameComparer : IEqualityComparer<Video>
        {
            public bool Equals(Video x, Video y)
            {
                int stringTolerance = 3;
                return Compute(x.Name, y.Name, stringTolerance) <= stringTolerance;

            }

            private int Compute(string string1, string string2, int tolerance)
            {
                if (String.IsNullOrEmpty(string1))
                {
                    if (!String.IsNullOrEmpty(string2))
                        return string2.Length;

                    return 0;
                }

                if (String.IsNullOrEmpty(string2))
                {
                    if (!String.IsNullOrEmpty(string1))
                        return string1.Length;

                    return 0;
                }

                int length1 = string1.Length;
                int length2 = string2.Length;

                int[,] d = new int[length1 + 1, length2 + 1];

                int cost, del, ins, sub;

                for (int i = 0; i <= d.GetUpperBound(0); i++)
                    d[i, 0] = i;

                for (int i = 0; i <= d.GetUpperBound(1); i++)
                    d[0, i] = i;

                for (int i = 1; i <= d.GetUpperBound(0); i++)
                {
                    for (int j = 1; j <= d.GetUpperBound(1); j++)
                    {
                        if (string1[i - 1] == string2[j - 1])
                            cost = 0;
                        else
                            cost = 1;

                        del = d[i - 1, j] + 1;
                        ins = d[i, j - 1] + 1;
                        sub = d[i - 1, j - 1] + cost;

                        d[i, j] = Math.Min(del, Math.Min(ins, sub));

                        if (i > 1 && j > 1 && string1[i - 1] == string2[j - 2] && string1[i - 2] == string2[j - 1])
                            d[i, j] = Math.Min(d[i, j], d[i - 2, j - 2] + cost);
                    }

                    if (d[d.GetUpperBound(0), d.GetUpperBound(1)] > tolerance)
                        return d[d.GetUpperBound(0), d.GetUpperBound(1)]; // return early
                }

                return d[d.GetUpperBound(0), d.GetUpperBound(1)];
            }

            public int GetHashCode([DisallowNull] Video obj)
            {
                return 1;
            }
        }
    }
}
