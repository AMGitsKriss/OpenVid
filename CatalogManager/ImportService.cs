using CatalogManager.Metadata;
using CatalogManager.Models;
using Database;
using Database.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogManager
{
    public class ImportService
    {
        private readonly CatalogImportOptions _configuration;
        private readonly IVideoRepository _repository;
        private readonly IMetadataStrategy _metadata;

        public ImportService(IOptions<CatalogImportOptions> configuration, IVideoRepository repository, IMetadataStrategy metadata)
        {
            _configuration = configuration.Value;
            _repository = repository;
            _metadata = metadata;
        }
        public List<FoundVideo> FindFiles()
        {
            var importDir = Path.Combine(_configuration.ImportDirectory, "01_pending");
            Directory.CreateDirectory(importDir);
            return FindFiles(importDir, importDir);
        }

        public List<FoundVideo> GetQueuedFiles()
        {
            var queue = _repository.GetEncodeQueue();

            return queue.Select(q => new FoundVideo() { 
                FileName = Path.GetFileNameWithoutExtension(q.InputDirectory),
                Resolution = $"{q.MaxHeight}p",
                FullName = q.InputDirectory
            }).ToList();
        }

        public List<FoundVideo> FindFiles(string dir, string prefix)
        {
            try
            {
                var result = new List<FoundVideo>();

                var suggestedTags = dir.Replace(prefix, "").Split(@"\", StringSplitOptions.RemoveEmptyEntries).ToList();

                // Files
                foreach (var file in Directory.GetFiles(dir))
                {
                    var info = new FileInfo(file);
                    var video = new FoundVideo()
                    {
                        FileName = info.Name,
                        FullName = info.FullName,
                        FileLocation = info.DirectoryName,
                        SuggestedTags = suggestedTags,
                        Size = info.Length
                    };
                    result.Add(video);
                }

                // Directories
                foreach (var folder in Directory.GetDirectories(dir))
                {
                    result.AddRange(FindFiles(folder, prefix));
                }

                return result;
            }
            catch (DirectoryNotFoundException)
            {

                return FindFiles(dir, prefix);
            }
        }

        public async Task<bool> UploadFile(IFormFile file)
        {
            string targetFolder = Path.Combine(_configuration.ImportDirectory, "01_pending");

            Helpers.FileHelpers.TouchDirectory(targetFolder);

            string targetPath = Path.Combine(targetFolder, file.FileName);
            using Stream fileStream = new FileStream(targetPath, FileMode.Create);

            try
            {
                await file.CopyToAsync(fileStream);
                return true;
            }
            catch (Exception ex)
            {
                File.Delete(targetPath);
                return false;
            }
        }

        public void QueueFiles()
        {
            var pendingFiles = FindFiles();
            var preset = _configuration.EncoderPresets.First();

            var queuedDirectory = Path.Combine(_configuration.ImportDirectory, "02_queued");
            var completeDirectory = Path.Combine(_configuration.ImportDirectory, "03_awaiting_move");

            foreach (var pending in pendingFiles)
            {
                var fileNameWithResolution = $"{Path.GetFileNameWithoutExtension(pending.FileName)}_{preset.MaxHeight}{Path.GetExtension(pending.FileName)}";

                // DATABASE
                var videoId = CreateVideoInDatabase(pending, preset, queuedDirectory, completeDirectory, fileNameWithResolution);
                if (videoId == 0)
                    continue;

                if (!MoveFileToDirectory(pending.FullName, queuedDirectory, fileNameWithResolution))
                    _repository.DeleteVideo(videoId);
            }
        }

        private int CreateVideoInDatabase(FoundVideo pending, EncoderPresetOptions preset, string queuedDirectory, string completeDirectory, string fileNameWithResolution)
        {
            // TODO - Suggested tags should get made here
            var queuedFullName = Path.Combine(queuedDirectory, fileNameWithResolution);
            var completeFullName = Path.Combine(completeDirectory, fileNameWithResolution);

            var meta = _metadata.GetMetadata(pending.FullName);
            var toSave = new Video()
            {
                Name = Path.GetFileNameWithoutExtension(pending.FileName),
                Length = meta.Duration,
                VideoEncodeQueue = new List<VideoEncodeQueue>()
                {
                    new VideoEncodeQueue()
                    {
                        VideoId = 0,
                        InputDirectory = queuedFullName,
                        OutputDirectory = completeFullName,
                        Encoder = preset.Encoder,
                        RenderSpeed = preset.RenderSpeed,
                        Format = preset.Format,
                        Quality = preset.Quality,
                        MaxHeight = preset.MaxHeight,
                        IsVertical = meta.Height > meta.Width
                    }
                }
            };

            try
            {
                toSave = _repository.SaveVideo(toSave);
            }
            catch (Exception ex)
            {
            }

            return toSave.Id;
        }

        private bool MoveFileToDirectory(string pendingFullName, string queuedDirectory, string fileNameWithResolution)
        {
            var queuedFullName = Path.Combine(queuedDirectory, fileNameWithResolution);

            try
            {
                if (File.Exists(pendingFullName))
                {
                    if (!Directory.Exists(queuedDirectory))
                        Directory.CreateDirectory(queuedDirectory);
                    File.Move(pendingFullName, queuedFullName);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}