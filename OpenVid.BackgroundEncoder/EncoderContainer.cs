using CatalogManager;
using CatalogManager.Encoder;
using CatalogManager.Metadata;
using Database;
using Database.Models;
using Microsoft.Extensions.Options;
using System.IO;
using System.Linq;
using System.Threading;
using CatalogManager.Helpers;

namespace OpenVid.BackgroundEncoder
{
    public class EncoderContainer
    {
        private readonly IVideoRepository _repository;
        private readonly IEncoderStrategy _encoder;
        private readonly IMetadataStrategy _metadata;
        private readonly CatalogImportOptions _configuration;
        private volatile bool _continueJob;

        public EncoderContainer(IVideoRepository repository, IEncoderStrategy encoder, IMetadataStrategy metadata, IOptions<CatalogImportOptions> configuration)
        {
            _repository = repository;
            _encoder = encoder;
            _metadata = metadata;
            _configuration = configuration.Value;
        }

        public void EncodeJob()
        {
            while (_continueJob)
            {
                // Get the oldest valid job in the queue.
                var queueItem = _repository.GetEncodeQueue().FirstOrDefault();

                // No more jobs to do!
                if (queueItem == null)
                {
                    _continueJob = false;
                    break;
                }

                _encoder.Run(queueItem);

                // If getting metadata works, then we know the file exists
                var metadata = _metadata.GetMetadata(queueItem.OutputDirectory);

                // Remove the old file
                File.Delete(queueItem.InputDirectory);

                // Write the new source entry
                var md5 = FileHelpers.GenerateHash(queueItem.OutputDirectory);
                var videoSource = new VideoSource()
                {
                    VideoId = queueItem.VideoId,
                    Md5 = md5,
                    Width = metadata.Width,
                    Height = metadata.Height,
                    Size = new FileInfo(queueItem.OutputDirectory).Length,
                    Extension = "mp4"
                };
                _repository.SaveVideoSource(videoSource);

                // Move the new file to the bucket
                string vidSubFolder = md5.Substring(0, 2);
                string videoDirectory = Path.Combine(_configuration.BucketDirectory, "video", vidSubFolder);
                string videoBucketDirectory = Path.Combine(videoDirectory, $"{md5}.mp4");
                File.Move(queueItem.OutputDirectory, videoBucketDirectory);

                // Generate the thumbnail
                string thumbSubFolder = queueItem.VideoId.ToString().PadLeft(2, '0').Substring(0, 2);
                string thumbDirectory = Path.Combine(_configuration.BucketDirectory, "thumbnail", thumbSubFolder);

                FileHelpers.TouchDirectory(thumbDirectory);

                string thumbPath = Path.Combine(thumbDirectory, $"{queueItem.VideoId.ToString().PadLeft(2, '0')}.jpg");
                if (!File.Exists(thumbPath))
                    _metadata.CreateThumbnail(videoBucketDirectory, thumbPath);

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
    }
}