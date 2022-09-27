using CatalogManager.Models;

namespace CatalogManager.Metadata
{
    public interface IMetadataStrategy
    {
        MediaProperties GetMetadata(string location);
        void CreateThumbnail(string videoPath, string thumbPath, int framesIntoVideo);
        void FindSubtitles(string source, string outputFolder);
    }
}
