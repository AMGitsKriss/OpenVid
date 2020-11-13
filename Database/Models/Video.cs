using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Video
    {
        public Video()
        {
            VideoTag = new HashSet<VideoTag>();
        }

        public int Id { get; set; }
        public string Md5 { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public TimeSpan Length { get; set; }
        public long Size { get; set; }

        public virtual ICollection<VideoTag> VideoTag { get; set; }
    }
}
