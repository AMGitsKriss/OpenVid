using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
#nullable disable

namespace Database.Models
{
    public partial class Tag
    {
        public Tag()
        {
            VideoTag = new HashSet<VideoTag>();
            TagImplicationFrom = new HashSet<TagImplication>();
            TagImplicationTo = new HashSet<TagImplication>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortCode { get; set; }
        public string Description { get; set; }
        public int? Type { get; set; }

        public virtual TagType TypeNavigation { get; set; }
        public virtual ICollection<VideoTag> VideoTag { get; set; }
        public virtual ICollection<TagImplication> TagImplicationFrom { get; set; }
        public virtual ICollection<TagImplication> TagImplicationTo { get; set; }
    }
}
