using CatalogManager;
using CatalogManager.Encoder;
using CatalogManager.Metadata;
using Database;
using Database.Models;
using Microsoft.Extensions.Options;
using System.IO;
using System.Linq;
using CatalogManager.Helpers;
using System;
using CatalogManager.Models;
using CatalogManager.Segment;

namespace OpenVid.BackgroundEncoder
{
    public class EncoderContainer
    {
        private readonly IVideoRepository _repository;
        private readonly IEncoderStrategy _encoder;
        private readonly IMetadataStrategy _metadata;
        private readonly ISegmenterStrategy _segmenter;
        private readonly CatalogImportOptions _configuration;
        private volatile bool _continueJob;

        public EncoderContainer(IVideoRepository repository, IEncoderStrategy encoder, IMetadataStrategy metadata, ISegmenterStrategy segmenter, IOptions<CatalogImportOptions> configuration)
        {
            _repository = repository;
            _encoder = encoder;
            _metadata = metadata;
            _segmenter = segmenter;
            _configuration = configuration.Value;
        }

        public void EncodeJob()
        {
            while (_continueJob)
            {
                // Get the oldest valid job in the queue.
                var queueItem = _repository.GetPendingEncodeQueue().FirstOrDefault();

                // No more jobs to do!
                if (queueItem == null)
                {
                    _continueJob = false;
                    break;
                }

                // TODO - Kaichou wa Maid-sama fails quietly
                _encoder.Run(queueItem);

                // If getting metadata works, then we know the file exists
                var metadata = _metadata.GetMetadata(queueItem.OutputDirectory);

                // Remove the old file
                if (!_repository.IsFileStillNeeded(queueItem.VideoId))
                {
                    try
                    {
                        File.Delete(queueItem.InputDirectory);
                    }
                    catch (Exception ex)
                    {
                        var msg = ex.Message;
                    }
                }

                CreateThumbnail(queueItem);

                // Write the new source entry
                if (queueItem.PlaybackFormat == "mp4")
                {
                    SaveMp4Video(queueItem, metadata);
                }
                else if (queueItem.PlaybackFormat == "dash")
                {
                    MoveDashVideoAwaitingPackager(queueItem);
                    AddToSegmentQueue(queueItem);
                }

                // Mark as done before looping
                queueItem.IsDone = true;
                _repository.SaveEncodeJob(queueItem);
            }
        }
        public void Stop()
        {
            _continueJob = false;
        }

        public void Start()
        {
            _continueJob = true;
            EncodeJob();
        }

        public bool IsRunning()
        {
            return _continueJob;
        }

        private void CreateThumbnail(VideoEncodeQueue queueItem)
        {
            string thumbSubFolder = queueItem.VideoId.ToString().PadLeft(2, '0').Substring(0, 2);
            string thumbDirectory = Path.Combine(_configuration.BucketDirectory, "thumbnail", thumbSubFolder);
            FileHelpers.TouchDirectory(thumbDirectory);

            string thumbPath = Path.Combine(thumbDirectory, $"{queueItem.VideoId.ToString().PadLeft(2, '0')}.jpg");
            if (!File.Exists(thumbPath))
                _metadata.CreateThumbnail(queueItem.OutputDirectory, thumbPath, _configuration.ThumbnailFramesIntoVideo);
        }

        private void SaveMp4Video(VideoEncodeQueue queueItem, MediaProperties metadata)
        {
            var md5 = FileHelpers.GenerateHash(queueItem.OutputDirectory);
            var videoSource = new VideoSource()
            {
                VideoId = queueItem.VideoId,
                Md5 = md5,
                Width = metadata.Width,
                Height = metadata.Height,
                Size = new FileInfo(queueItem.OutputDirectory).Length,
                Extension = Path.GetExtension(queueItem.OutputDirectory).Replace(".", "")
            };
            _repository.SaveVideoSource(videoSource);

            // Move the new file to the bucket
            string vidSubFolder = md5.Substring(0, 2);
            string videoDirectory = Path.Combine(_configuration.BucketDirectory, "video", vidSubFolder);
            FileHelpers.TouchDirectory(videoDirectory);
            string videoBucketDirectory = Path.Combine(videoDirectory, $"{md5}.{Path.GetExtension(queueItem.OutputDirectory)}");
            File.Move(queueItem.OutputDirectory, videoBucketDirectory);
        }

        private void MoveDashVideoAwaitingPackager(VideoEncodeQueue queueItem)
        {
            var segmentedDirectory = Path.Combine(_configuration.ImportDirectory, "04_shaka_packager", Path.GetFileNameWithoutExtension(queueItem.InputDirectory));
            var segmentedFullName = Path.Combine(segmentedDirectory, Path.GetFileName(queueItem.OutputDirectory));
            FileHelpers.TouchDirectory(segmentedDirectory);
            File.Move(queueItem.OutputDirectory, segmentedFullName);
        }

        private void AddToSegmentQueue(VideoEncodeQueue queueItem)
        {
            var segmentedDirectory = Path.Combine(_configuration.ImportDirectory, "04_shaka_packager", Path.GetFileNameWithoutExtension(queueItem.InputDirectory));
            var segmentedFullName = Path.Combine(segmentedDirectory, Path.GetFileName(queueItem.OutputDirectory));

            var job = new VideoSegmentQueue()
            {
                VideoId = queueItem.VideoId,
                Height = queueItem.MaxHeight,
                InputDirectory = segmentedFullName
            };

            _repository.SaveSegmentJob(job);
        }

    }
}