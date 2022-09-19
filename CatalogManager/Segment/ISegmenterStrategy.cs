using Database.Models;
using System.Collections.Generic;

namespace CatalogManager.Segment
{
    public interface ISegmenterStrategy
    {
        void Segment(List<VideoSegmentQueue> videosToSegment);
    }
}
