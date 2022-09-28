using CatalogManager.Models;
using System.Collections.Generic;

namespace CatalogManager.Metadata
{
    public interface IMetadataStrategy
    {
        MediaProperties GetMetadata(string location);
        void CreateThumbnail(string videoPath, string thumbPath, int framesIntoVideo);
        IEnumerable<SubtitleFile> FindSubtitles(string source, string outputFolder);
    }
}
