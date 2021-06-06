using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Ratings
    {
        public Ratings()
        {
            Video = new HashSet<Video>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Video> Video { get; set; }
    }
}
