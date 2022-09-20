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
            var queue = _repository.GetPendingEncodeQueue();

            return queue.Select(q => new FoundVideo()
            {
                FileName = Path.GetFileNameWithoutExtension(q.InputDirectory),
                Resolution = $"{q.MaxHeight}p",
                FullName = q.InputDirectory,
                PlaybackFormat = q.PlaybackFormat
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

            var queuedDirectory = Path.Combine(_configuration.ImportDirectory, "02_queued");
            var completeDirectory = Path.Combine(_configuration.ImportDirectory, "03_awaiting_move");

            foreach (var pending in pendingFiles)
            {
                // DATABASE
                var videoId = CreateVideoInDatabase(pending, queuedDirectory, completeDirectory);
                if (videoId == 0)
                    continue;

                if (!MoveFileToDirectory(pending.FullName, queuedDirectory, Path.Combine(queuedDirectory, pending.FileName)))
                    _repository.DeleteVideo(videoId);
            }
        }

        private int CreateVideoInDatabase(FoundVideo pending, string queuedDirectory, string outputDirectory)
        {
            var tags = _repository.DefineTags(pending.SuggestedTags);

            var inputFullName = Path.Combine(queuedDirectory, pending.FileName);
            

            var meta = _metadata.GetMetadata(pending.FullName);
            var toSave = new Video()
            {
                Name = Path.GetFileNameWithoutExtension(pending.FileName),
                Length = meta.Duration,
                VideoEncodeQueue = new List<VideoEncodeQueue>(),
                VideoTag = tags.Select(t => new VideoTag()
                {
                    Tag = t
                }).ToList()
            };

            // If our source is 720p, don't bother trying to use the 1080p preset.
            var presets = GetPresets(pending.FullName);

            foreach (var preset in presets)
            {
                var outputFullName = Path.Combine(outputDirectory, $"{Path.GetFileNameWithoutExtension(pending.FileName)}_{preset.MaxHeight}.mp4");
                toSave.VideoEncodeQueue.Add(new VideoEncodeQueue()
                {
                    VideoId = 0,
                    InputDirectory = inputFullName,
                    OutputDirectory = outputFullName,
                    Encoder = preset.Encoder,
                    RenderSpeed = preset.RenderSpeed,
                    VideoFormat = preset.VideoFormat,
                    PlaybackFormat = preset.PlaybackFormat,
                    Quality = preset.Quality,
                    MaxHeight = preset.MaxHeight,
                    IsVertical = meta.Height > meta.Width
                });
            }

            try
            {
                toSave = _repository.SaveVideo(toSave);
            }
            catch (Exception ex)
            {
            }

            return toSave.Id;
        }

        private List<EncoderPresetOptions> GetPresets(string fileFullName)
        {
            // TODO - This is gnarly. Is there a nicer way to do it?
            var results = new List<EncoderPresetOptions>();
            var metadata = _metadata.GetMetadata(fileFullName);

            var mp4Presets = _configuration.EncoderPresets.Where(v => v.MaxHeight <= metadata.Height && v.PlaybackFormat == "mp4").ToList();
            var smalledmp4Preset = _configuration.EncoderPresets.Where(v => v.PlaybackFormat == "mp4").OrderByDescending(v => v.MaxHeight).FirstOrDefault();
            if (!mp4Presets.Any() && smalledmp4Preset != null)
                results.Add(smalledmp4Preset);
            else
                results.AddRange(mp4Presets);

            var mpdPresets = _configuration.EncoderPresets.Where(v => v.MaxHeight <= metadata.Height && v.PlaybackFormat == "dash").ToList();
            var smalledmpdPreset = _configuration.EncoderPresets.Where(v => v.PlaybackFormat == "dash").OrderByDescending(v => v.MaxHeight).FirstOrDefault();
            if (!mpdPresets.Any() && smalledmpdPreset != null)
                results.Add(smalledmpdPreset);
            else
                results.AddRange(mpdPresets);

            return results;
        }

        private bool MoveFileToDirectory(string sourceFullName, string targetDirectory, string destinationFullName)
        {
            try
            {
                if (File.Exists(sourceFullName))
                {
                    if (!Directory.Exists(targetDirectory))
                        Directory.CreateDirectory(targetDirectory);
                    File.Move(sourceFullName, destinationFullName);
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