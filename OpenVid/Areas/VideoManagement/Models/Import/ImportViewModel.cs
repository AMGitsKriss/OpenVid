using OpenVid.Models;
using System.Collections.Generic;

namespace OpenVid.Areas.VideoManagement.Models.Import
{
    public class ImportViewModel : BaseViewModel
    {
        public List<FoundVideoViewModel> DiscoveredFiles { get; set; }
    }

    public class FoundVideoViewModel
    {
        public string FileName { get; set; }
        public string FullName { get; set; }
        public string FileLocation { get; set; }
        public List<string> SuggestedTags { get; set; }
    }
}
