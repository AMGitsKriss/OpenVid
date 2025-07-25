﻿using CatalogManager.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CatalogManager.Metadata
{
    public interface IMetadataStrategy
    {
        MediaProperties GetMetadata(string location);
        Task CreateThumbnail(string videoPath, string thumbPath, TimeSpan timeIntoVideo, int? timeout = null);
        Task CreateThumbnail(int width, int height, string videoPath, string thumbPath, TimeSpan timeIntoVideo, int? timeout = null);
        IEnumerable<SubtitleFile> FindSubtitles(string source);
        void ExtractSubtitles(SubtitleFile subtitleFiles, string outputFolder, bool convertToVtt = true);
    }
}
