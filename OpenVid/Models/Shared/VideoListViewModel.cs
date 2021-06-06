using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenVid.Models.Shared
{
    public class VideoListViewModel
    {
        public List<Database.Models.Video> Videos { get; set; }
        public int NextPageNumber { get; set; }
        public string SearchQuery { get; internal set; }
        public bool HasNextPage { get; internal set; }
    }
}
