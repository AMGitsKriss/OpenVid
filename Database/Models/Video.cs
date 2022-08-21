using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Database.Models
{
    public partial class Video
    {
        public Video()
        {
            VideoSource = new HashSet<VideoSource>();
            VideoTag = new HashSet<VideoTag>();
        }

        public int Id { get; set; }
        public string Md5 { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public TimeSpan Length { get; set; }
        public long? Size { get; set; }
        public string Description { get; set; }
        public string MetaText { get; set; }
        public bool IsDeleted { get; set; }
        public int? RatingId { get; set; }

        public virtual Ratings Rating { get; set; }
        public virtual ICollection<VideoSource> VideoSource { get; set; }
        public virtual ICollection<VideoTag> VideoTag { get; set; }
    }
}
