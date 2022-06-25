using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenVid.Models.Import
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
