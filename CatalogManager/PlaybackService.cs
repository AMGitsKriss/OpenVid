using CatalogManager.Helpers;
using CatalogManager.Metadata;
using Database;
using Microsoft.Extensions.Options;
using System.IO;
using System.Linq;

namespace CatalogManager
{
    public class PlaybackService
    {
        private readonly CatalogImportOptions _configuration;
        private readonly IVideoRepository _repository;
        private readonly IMetadataStrategy _metadata;

        public PlaybackService(IOptions<CatalogImportOptions> configuration, IVideoRepository repository, IMetadataStrategy metadata)
        {
            _configuration = configuration.Value;
            _repository = repository;
            _metadata = metadata;
        }

        public void TryGenerateThumbnail(int id)
        {
            var video = _repository.GetVideo(id);
            var source = video.VideoSource.FirstOrDefault(s => s.Extension == "mp4");

            if (source == null) 
                return;

            var idString = id.ToString().PadLeft(2, '0');

            var videoPath = Path.Combine(_configuration.BucketDirectory, "video", source.Md5.Substring(0, 2), $"{source.Md5}.{source.Extension}");
            var thumbnailTarget = Path.Combine(_configuration.BucketDirectory, "thumbnail", idString.Substring(0, 2), $"{idString}.jpg");

            FileHelpers.TouchDirectory(Path.Combine(_configuration.BucketDirectory, "thumbnail"));
            FileHelpers.TouchDirectory(Path.Combine(_configuration.BucketDirectory, "thumbnail", idString.Substring(0, 2)));

            _metadata.CreateThumbnail(videoPath, thumbnailTarget, _configuration.ThumbnailFramesIntoVideo);
        }
    }
}
