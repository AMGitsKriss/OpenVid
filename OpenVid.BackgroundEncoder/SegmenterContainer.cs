using CatalogManager;
using CatalogManager.Encoder;
using CatalogManager.Helpers;
using CatalogManager.Metadata;
using CatalogManager.Segment;
using Database;
using Database.Models;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;

namespace OpenVid.BackgroundEncoder
{
    public class SegmenterContainer
    {
        private readonly IVideoRepository _repository;
        private readonly IEncoderStrategy _encoder;
        private readonly IMetadataStrategy _metadata;
        private readonly ISegmenterStrategy _segmenter;
        private readonly CatalogImportOptions _configuration;
        private volatile bool _continueJob;

        public SegmenterContainer(IVideoRepository repository, IEncoderStrategy encoder, IMetadataStrategy metadata, ISegmenterStrategy segmenter, IOptions<CatalogImportOptions> configuration)
        {
            _repository = repository;
            _encoder = encoder;
            _metadata = metadata;
            _segmenter = segmenter;
            _configuration = configuration.Value;
        }

        public void SegmentJob()
        {
            while (_continueJob)
            {
                // Get the oldest valid job in the queue.
                var queueItems = _repository.GetPendingSegmentQueue().FirstOrDefault();
                var videoId = queueItems.First().VideoId;

                // No more jobs to do!
                if (queueItems == null)
                {
                    _continueJob = false;
                    break;
                }

                _segmenter.Segment(queueItems.ToList());
                _repository.SetPendingSegmentingDone(videoId);

                foreach (var item in queueItems)
                {
                    try
                    {
                        File.Delete(item.InputDirectory);
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }
                }

                string md5;
                var segmentedDirectory = Path.GetDirectoryName(queueItems.First().InputDirectory);
                string videoManifestDir = Path.Combine(segmentedDirectory, "dash.mpd");
                string videoPlaylistDir = Path.Combine(segmentedDirectory, "hls.m3u8");

                DirectoryInfo dirInfo = new DirectoryInfo(segmentedDirectory);
                long dirSize = dirInfo.GetFiles().Sum(f => f.Length);

                // Create a source for MPD
                md5 = FileHelpers.GenerateHash(videoManifestDir);
                var dashSource = new VideoSource()
                {
                    VideoId = videoId,
                    Md5 = md5,
                    Extension = "mpd",
                    Size = dirSize
                };

                // Create a source for M3U8
                var hlsSource = new VideoSource()
                {
                    VideoId = videoId,
                    Md5 = md5,
                    Extension = "m3u8",
                    Size = dirSize
                };

                _repository.SaveVideoSource(dashSource);
                _repository.SaveVideoSource(hlsSource);

                string vidSubFolder = md5.Substring(0, 2);
                string videoDirectory = Path.Combine(_configuration.BucketDirectory, "video", vidSubFolder, md5);
                Directory.Move(segmentedDirectory, videoDirectory);
            }

        }

        public void Stop()
        {
            _continueJob = false;
        }

        public void Start()
        {
            _continueJob = true;
            SegmentJob();
        }

        public bool IsRunning()
        {
            return _continueJob;
        }
    }
}
