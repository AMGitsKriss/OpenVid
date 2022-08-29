using Database.Models;
using System.Collections.Generic;
using System.Linq;

namespace Database
{
    public interface IVideoRepository
    {
        IQueryable<Tag> DefineTags(List<string> tags);
        bool SoftDelete(int id);
        IQueryable<Tag> GetAllTags();
        IQueryable<Video> GetViewableVideos();
        List<Ratings> GetRatings();
        Video GetVideo(int id);
        Video GetVideo(string md5);
        Video SaveVideo(Video video);
        IQueryable<Video> Search(string search);
        IQueryable<VideoTag> TagsWithVideos();
        VideoSource GetVideoSource(string md5);
        VideoSource SaveVideoSource(VideoSource toSave);
        bool DeleteVideo(int id);
        bool DeleteSource(string md5);
        IQueryable<Video> GetSoftDeletedVideos();
        void RemoveTagsFromVideo(IEnumerable<VideoTag> removeTags);
        void AddTagsToVideo(IEnumerable<VideoTag> removeTags);
        bool SaveEncodeJob(VideoEncodeQueue encodeJob);
        IQueryable<VideoEncodeQueue> GetEncodeQueue();
    }
}