using Database.Models;

namespace CatalogManager.Encoder
{
    public interface IEncoderStrategy
    {
        void Run(VideoEncodeQueue queueItem);
    }
}