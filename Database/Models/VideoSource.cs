using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class VideoSource
    {
        public int Id { get; set; }
        public int? VideoId { get; set; }
        public string Md5 { get; set; }
        public string Extension { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public long Size { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Video Video { get; set; }
    }
}
