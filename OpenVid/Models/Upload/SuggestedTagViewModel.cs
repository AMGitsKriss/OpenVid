using System.Collections.Generic;

namespace OpenVid.Models.Upload
{
    public class SuggestedTagViewModel
    {
        public string TagName { get; set; }
        public List<RelatedTagViewModel> RelatedTags { get; set; }
    }

    public class RelatedTagViewModel
    {
        public string TagName { get; set; }
        public bool AlreadyUsed { get; set; }
    }
}