using System.Collections.Generic;

namespace OpenVid.Areas.Tags.Models.Management
{
    public class TagImplicationViewModel
    {
        public int Id { get; set; }
        public string Name {  get; set; }
        public string Type { get; set; }
        public Dictionary<int, string> Implications { get; set; }
    }
}
