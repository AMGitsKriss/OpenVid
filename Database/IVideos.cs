using Database.Models;
using System.Collections.Generic;
using System.Linq;

namespace Database
{
    public interface IVideos
    {
        IQueryable<Tag> DefineTags(List<string> tags);
        bool SoftDelete(int id);
        IQueryable<Tag> GetAllTags();
        IQueryable<Video> GetAllVideos();
        IQueryable<Video> GetDeletedVideos();
        List<Ratings> GetRatings();
        Video GetVideo(int id);
        Video GetVideo(string md5);
        IEnumerable<Tag> SaveTagsForVideo(Video video, IEnumerable<Tag> tags);
        Video SaveVideo(Video video);
        IQueryable<Video> Search(string search);
        IQueryable<VideoTag> VideosByTag();
    }
}