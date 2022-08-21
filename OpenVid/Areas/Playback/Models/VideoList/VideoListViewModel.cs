using OpenVid.Models;
using OpenVid.Models.Shared;
using System.Collections.Generic;

namespace OpenVid.Areas.Playback.Models.VideoList
{
    public class VideoListViewModel : BaseViewModel
    {
        public List<VideoViewModel> Videos { get; set; }
        public int NextPageNumber { get; set; }
        public string SearchQuery { get; internal set; }
        public bool HasNextPage { get; internal set; }
    }
}
