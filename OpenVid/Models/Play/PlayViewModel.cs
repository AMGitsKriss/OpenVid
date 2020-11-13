using Database.Models;
using OpenVid.Models.Upload;

namespace OpenVid.Models.Play
{
    public class PlayViewModel : BaseViewModel
    {
        public Video Video { get; set; }
        public UpdateFormViewModel Update { get; set; }
    }
}
