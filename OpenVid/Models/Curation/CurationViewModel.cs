using OpenVid.Models.Shared;
using System.Collections.Generic;

namespace OpenVid.Models.Curation
{
    public class CurationViewModel : BaseViewModel
    {
        public List<VideoViewModel> VideosForDeletion { get; internal set; }
    }
}
