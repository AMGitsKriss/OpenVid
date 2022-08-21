using Database.Models;
using System.Collections.Generic;

namespace OpenVid.Models.Home
{
    public class HomeViewModel : BaseViewModel
    {
        public List<Tag> Tags { get; set; }
    }
}
