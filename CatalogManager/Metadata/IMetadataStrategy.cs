using CatalogManager.Models;
using System;
using System.Collections.Generic;

namespace CatalogManager.Metadata
{
    public interface IMetadataStrategy
    {
        MediaProperties GetMetadata(string location);
        void CreateThumbnail(string videoPath, string thumbPath, TimeSpan timeIntoVideo);
        IEnumerable<SubtitleFile> FindSubtitles(string source, string outputFolder);
        void ExtractSubtitles(SubtitleFile subtitleFiles);
    }
}
