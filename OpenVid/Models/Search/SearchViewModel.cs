using Database.Models;
using System.Collections.Generic;

namespace OpenVid.Models.Search
{
    public class SearchViewModel : BaseViewModel
    {
        public List<Tag> Tags { get; set; }
        public string SearchString { get; set; }
    }
}
