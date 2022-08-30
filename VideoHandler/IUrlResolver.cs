using Database.Models;
using System.Collections.Generic;

namespace VideoHandler
{
    public interface IUrlResolver
    {
        List<string> GetVideoUrl(Video video);
    }
}