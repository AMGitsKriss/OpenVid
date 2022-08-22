using OpenVid.Areas.Playback.Models.Update;
using OpenVid.Models;
using System.Collections.Generic;

namespace OpenVid.Areas.Playback.Models.Play
{
    public class PlayViewModel : BaseViewModel
    {
        public List<string> VideoSources { get; set; }
        public UpdateFormViewModel Update { get; set; }
    }
}
