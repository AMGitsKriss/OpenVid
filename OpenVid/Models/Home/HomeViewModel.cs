using Database.Models;
using OpenVid.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenVid.Models.Home
{
    public class HomeViewModel : BaseViewModel
    {
        public VideoListViewModel Videos { get; set; }
        public List<Tag> Tags { get; set; }
    }
}
