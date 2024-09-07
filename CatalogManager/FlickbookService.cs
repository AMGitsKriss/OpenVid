using CatalogManager.Helpers;
using CatalogManager.Metadata;
using Database;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using System.Threading;
using System.Collections.Concurrent;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace CatalogManager
{
    public class FlickbookService
    {
        private readonly CatalogImportOptions _configuration;
        private readonly IVideoRepository _repository;
        private readonly IMetadataStrategy _metadata;
        private readonly ILogger _logger;
        private readonly DbContextOptions<OpenVidContext> _dbOptions;

        private readonly ConcurrentQueue<int> _thumbnailTasks = new();
        private Thread _thread = null;

        public FlickbookService(ILogger logger, IOptions<CatalogImportOptions> configuration, IVideoRepository repository, IMetadataStrategy metadata, DbContextOptions<OpenVidContext> dbOptions)
        {
            _configuration = configuration.Value;
            _repository = repository;
            _metadata = metadata;
            _logger = logger;
            _dbOptions = dbOptions;
        }

        public async Task TryGenerateThumbnail(int id)
        {
            if (!_thumbnailTasks.Contains(id))
            {
                _thumbnailTasks.Enqueue(id);
                //new Thread(async () => await ThumbnailThread());
                Task.Run(() => InvokeService(id));
            }
        }

        public async Task InvokeService(int id)
        {
            _logger.Information($"Attempting to generate flickbook for {id}.");

            var repo = new VideoRepository(new OpenVidContext(_dbOptions));
            var video = repo.GetVideo(id);
            var source = video.VideoSource.FirstOrDefault(s => s.Extension == "mp4" || s.Extension == "webm");

            if (source == null)
                return;

            var idString = id.ToString().PadLeft(2, '0');

            var videoPath = Path.Combine(_configuration.BucketDirectory, "video", source.Md5.Substring(0, 2), $"{source.Md5}.{source.Extension}");

            FileHelpers.TouchDirectory(Path.Combine(_configuration.BucketDirectory, "flickbook"));
            FileHelpers.TouchDirectory(Path.Combine(_configuration.BucketDirectory, "flickbook", idString.Substring(0, 2)));
            FileHelpers.TouchDirectory(Path.Combine(_configuration.BucketDirectory, "flickbook", idString.Substring(0, 2), idString));

            var intrevals = new List<int>() { 0, 1, 2, 3, 4, 5 };

            if (video.Length.TotalSeconds >= 30)
                intrevals.Add(30);

            for (int i = 0; i < video.Length.TotalMinutes; i++)
            {
                if (video.Length.TotalSeconds <= 5)
                    break;
                intrevals.Add(i * 60);
            }

            var startTime = DateTime.Now;
            foreach (var t in intrevals)
            {
                var videoSource = video.VideoSource.OrderBy(s => s.Width).First();
                var src = Path.Combine(_configuration.BucketDirectory, "flickbook", idString.Substring(0, 2), idString, $"{t:00000}.jpg");
                await _metadata.CreateThumbnail(videoSource.Width, videoSource.Height, videoPath, src, TimeSpan.FromSeconds(t));
            }
            var duration = DateTime.Now - startTime;
            _logger.Information($"Completed to generation of flickbook for {id}. Duration: {duration}");
        }
    }
}
