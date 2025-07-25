﻿using CatalogManager.Helpers;
using CatalogManager.Metadata;
using Database;
using Microsoft.Extensions.Options;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
    public class ThumbnailService
    {
        private readonly CatalogImportOptions _configuration;
        private readonly IVideoRepository _repository;
        private readonly IMetadataStrategy _metadata;
        private readonly ILogger _logger;
        private readonly DbContextOptions<OpenVidContext> _dbOptions;

        private readonly ConcurrentQueue<int> _thumbnailTasks = new();
        private Thread _thread = null;

        public ThumbnailService(ILogger logger, IOptions<CatalogImportOptions> configuration, IVideoRepository repository, IMetadataStrategy metadata, DbContextOptions<OpenVidContext> dbOptions)
        {
            _configuration = configuration.Value;
            _repository = repository;
            _metadata = metadata;
            _logger = logger;
            _dbOptions = dbOptions;
        }

        public async Task TryGenerateThumbnail(int id)
        {
            /*
            try
            {
                if (!_thumbnailTasks.Contains(id))
                    _thumbnailTasks.Enqueue(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }

            var state = _thread == null ? null : _thread.ThreadState.ToString();
            _logger.Information($"Thumbnail generation thread state: {state}");

            if (_thread == null || _thread.ThreadState != ThreadState.Running || !_thread.IsAlive)
            {
                _logger.Information($"Restarting thread.");
                _thread = new Thread(async () => await ThumbnailThread());
                _thread.Start();
            }
            */

            if (!_thumbnailTasks.Contains(id))
            {
                _thumbnailTasks.Enqueue(id);
                //new Thread(async () => await ThumbnailThread());
                Task.Run(() => InvokeService(id));
            }

            //Task.Run(() => _metadata.CreateThumbnail(videoPath, thumbnailTarget, thumbTimespan));
        }

        public async Task ThumbnailThread()
        {
            while (_thumbnailTasks.Any())
            {
                var readSuccess = _thumbnailTasks.TryDequeue(out var id);
                Thread.Sleep(2000);

                if (!readSuccess)
                {
                    break;
                }

                InvokeService(id);
            }
        }

        public async Task InvokeService(int id)
        {
            _logger.Information($"Attempting to generate thumbnail for {id}.");

            var repo = new VideoRepository(new OpenVidContext(_dbOptions));
            var video = repo.GetVideo(id);
            var source = video.VideoSource.FirstOrDefault(s => s.Extension == "mp4" || s.Extension == "webm");

            if (source == null)
                return;

            var idString = id.ToString().PadLeft(2, '0');

            var videoPath = Path.Combine(_configuration.BucketDirectory, "video", source.Md5.Substring(0, 2), $"{source.Md5}.{source.Extension}");
            var thumbnailTarget = Path.Combine(_configuration.BucketDirectory, "thumbnail", idString.Substring(0, 2), $"{idString}.jpg");

            FileHelpers.TouchDirectory(Path.Combine(_configuration.BucketDirectory, "thumbnail"));
            FileHelpers.TouchDirectory(Path.Combine(_configuration.BucketDirectory, "thumbnail", idString.Substring(0, 2)));

            // Thumbnail timestamp = Start 1 sec in for every 4 mins of length
            var start = 0d;
            if (video.Length.TotalMinutes > 1)
                start = 5;
            if(video.Length.TotalMinutes > 4)
                start = 5 + (video.Length.TotalMinutes / 4);

            var thumbTimespan = TimeSpan.FromSeconds(start);

            var startTime = DateTime.Now;
            await _metadata.CreateThumbnail(videoPath, thumbnailTarget, thumbTimespan);
            var duration = DateTime.Now - startTime;

            _logger.Information($"Completed to generation of thumbnail for {id}. Duration: {duration}");
        }

        public void SaveThumbnailForVideo(int id, byte[] image)
        {
            if (image == null)
                throw new FileNotFoundException("No image was included in the upload");

            var originalImage = Image.FromStream(new MemoryStream(image));
            var temp = FixedSize(originalImage, 300, 168);
            var newImage = new Bitmap(temp);

            var idString = id.ToString().PadLeft(2, '0');
            var thumbnailFolder = Path.Combine(_configuration.BucketDirectory, "thumbnail", idString.Substring(0, 2));
            var manualThumbnailFile = $"m{idString}.jpg";

            FileHelpers.TouchDirectory(thumbnailFolder);
            var manualThumbnailFullName = Path.Combine(thumbnailFolder, manualThumbnailFile);

            newImage.Save(manualThumbnailFullName);
        }

        public void DeleteThumbnailForVideo(int id)
        {
            var idString = id.ToString().PadLeft(2, '0');
            var thumbnailFolder = Path.Combine(_configuration.BucketDirectory, "thumbnail", idString.Substring(0, 2));
            var manualThumbnailFile = $"m{idString}.jpg";

            var manualThumbnailFullName = Path.Combine(thumbnailFolder, manualThumbnailFile);

            File.Delete(manualThumbnailFullName);
        }

        private Image FixedSize(Image image, int targetWidth, int targetHeight)
        {
            int sourceWidth = image.Width;
            int sourceHeight = image.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)targetWidth / (float)sourceWidth);
            nPercentH = ((float)targetHeight / (float)sourceHeight);
            if (nPercentH > nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((targetWidth -
                              (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((targetHeight -
                              (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(targetWidth, targetHeight,
                              PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(image.HorizontalResolution,
                             image.VerticalResolution);

            using (Graphics grPhoto = Graphics.FromImage(bmPhoto))
            {
                grPhoto.Clear(Color.Red);
                grPhoto.InterpolationMode =
                        InterpolationMode.HighQualityBicubic;

                grPhoto.DrawImage(image,
                    new Rectangle(destX, destY, destWidth, destHeight),
                    new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                    GraphicsUnit.Pixel);
            }

            return bmPhoto;
        }
    }
}
