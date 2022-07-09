using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenVid.Models.Shared
{
    public class VideoListViewModel : BaseViewModel
    {
        public List<VideoViewModel> Videos { get; set; }
        public int NextPageNumber { get; set; }
        public string SearchQuery { get; internal set; }
        public bool HasNextPage { get; internal set; }
    }

    public class VideoViewModel
    {
        public int Id { get; internal set; }
        public string Name { get; internal set; }
        public string Md5 { get; internal set; }
        public int SizeMb { get; internal set; }
        public string ThumbnailUrl { get; internal set; }
        public string Length { get; internal set; }
    }
}
