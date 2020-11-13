using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class VideoTag
    {
        public int VideoId { get; set; }
        public int TagId { get; set; }

        public virtual Tag Tag { get; set; }
        public virtual Video Video { get; set; }
    }
}
