using Database.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Search
{
    class Comparer : IEqualityComparer<Video>
    {
        public bool Equals(Video x, Video y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode([DisallowNull] Video obj)
        {
            return obj.Id;
        }
    }
}
