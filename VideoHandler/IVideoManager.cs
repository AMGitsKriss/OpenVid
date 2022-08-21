using Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using VideoHandler.Models;

namespace VideoHandler
{
    public interface IVideoManager
    {
        List<Ratings> GetRatings();
        Video GetVideo(int id);
        Task<SaveVideoResponse> ImportVideoAsync(ImportVideoRequest request);
        Task<SaveVideoResponse> SaveVideoAsync(SaveVideoRequest request);
        void UpdateMeta(string md5);
        IEnumerable<Video> GetSoftDeletedVideos();
        IEnumerable<Tag> GetAllTags();
        bool HardDeleteVideo(int id);
        IEnumerable<Tag> SaveTagsForVideo(Video video, IEnumerable<Tag> tags);
        IEnumerable<Tag> DefineTags(List<string> suggestedTags);
        Video SaveVideo(Video toSave);
        bool SoftDelete(int id);
        IEnumerable<Video> GetVideos();
    }
}