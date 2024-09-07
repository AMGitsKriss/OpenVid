using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
#nullable disable

namespace Database.Models
{
    public partial class TagImplication
    {
        public int FromId { get; set; }
        public int ToId { get; set; }
        public virtual Tag From { get; set; }
        public virtual Tag To { get; set; }
    }
}
