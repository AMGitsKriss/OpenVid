using System;
using System.Collections.Generic;

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
        public string Name { get; set; }
        public TimeSpan Length { get; set; }
        public string Description { get; set; }
        public string MetaText { get; set; }
        public bool IsDeleted { get; set; }
        public int? RatingId { get; set; }

        public virtual Ratings Rating { get; set; }
        public virtual ICollection<VideoSource> VideoSource { get; set; }
        public virtual ICollection<VideoTag> VideoTag { get; set; }
    }
}
