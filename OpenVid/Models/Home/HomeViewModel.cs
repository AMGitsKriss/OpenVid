using Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenVid.Models.Home
{
    public class HomeViewModel : BaseViewModel
    {
        public List<Video> Videos { get; set; }
        public List<Tag> Tags { get; set; }
    }
}
