using System.Collections.Generic;

namespace OpenVid.Models.Upload
{
    public class SuggestedTagViewModel
    {
        public string TagName { get; set; }
        public List<string> RelatedTags { get; set; }
    }
}