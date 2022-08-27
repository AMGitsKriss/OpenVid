using Database.Models;
using OpenVid.Models;
using System.Collections.Generic;

namespace OpenVid.Areas.Playback.Models.Search
{
    public class SearchViewModel : BaseViewModel
    {
        public List<Tag> Tags { get; set; }
    }
}
