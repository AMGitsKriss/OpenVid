using OpenVid.Models;
using OpenVid.Models.Upload;
using System.Collections.Generic;

namespace OpenVid.Areas.Playback.Models.Play
{
    public class PlayViewModel : BaseViewModel
    {
        public List<string> VideoSources { get; set; }
        public UpdateFormViewModel Update { get; set; }
    }
}
