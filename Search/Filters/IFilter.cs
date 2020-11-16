using Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Search.Filters
{
    public interface IFilter
    {
        List<Video> Filter(SearchParameter parameter);
    }
}
