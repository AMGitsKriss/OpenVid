using Database.Models;
using System.Collections.Generic;

namespace VideoHandler
{
    public interface IUrlResolver
    {
        string GetThumbnailUrl(Video video);
        List<string> GetVideoUrl(Video video);
    }
}