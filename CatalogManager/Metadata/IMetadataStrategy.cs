using CatalogManager.Models;

namespace CatalogManager.Metadata
{
    public interface IMetadataStrategy
    {
        MediaProperties GetMetadata(string location);
        void CreateThumbnail(string videoPath, string thumbPath);
    }
}
