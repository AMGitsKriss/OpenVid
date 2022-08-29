using CatalogManager.Models;
using Database;
using Database.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogManager
{
    public class ImportService
    {
        private readonly CatalogImportOptions _configuration;
        private readonly IVideoRepository _repository;

        public ImportService(IOptions<CatalogImportOptions> configuration, IVideoRepository repository)
        {
            _configuration = configuration.Value;
            _repository = repository;
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

        public void StopEncode()
        {
            throw new NotImplementedException();
        }

        public void StartEncode()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UploadFile(IFormFile file)
        {
            string targetFolder = Path.Combine(_configuration.ImportDirectory, "01_pending");

            TouchDirectory(targetFolder);

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
                // DATABASE
                var videoId = CreateVideoInDatabase(pending, preset, queuedDirectory, completeDirectory);
                if (videoId == 0)
                    continue;



                if (!MoveFileToDirectory(preset, pending.FileName, pending.FullName, queuedDirectory, completeDirectory))
                    _repository.DeleteVideo(videoId);
            }
        }

        private int CreateVideoInDatabase(FoundVideo pending, EncoderPresetOptions preset, string queuedDirectory, string completeDirectory)
        {
            // TODO - Suggested tags should get made here
            var queuedFullName = Path.Combine(queuedDirectory, pending.FileName);
            var completeFullName = Path.Combine(completeDirectory, pending.FileName);

            var meta = GetMetadata(pending.FullName);
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
                        MaxHeight = preset.MaxHeight
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

        private bool MoveFileToDirectory(EncoderPresetOptions preset, string pendingFileName, string pendingFullName, string queuedDirectory, string completeDirectory)
        {
            var queuedFileName = $"{Path.GetFileNameWithoutExtension(pendingFileName)}_{preset.MaxHeight}.mp4";
            var queuedFullName = Path.Combine(queuedDirectory, queuedFileName);

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

        private void TouchDirectory(string directory)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        private MediaProperties GetMetadata(string location)
        {
            // TODO - Try catch. The path can be wrong and we don't want to it fall over vaguely.
            string cmd = $"-v error -select_streams v:0 -show_entries stream=width,height,duration -show_entries format=duration -of csv=s=x:p=0 \"{location}\"";
            Process proc = new Process();
            proc.StartInfo.FileName = @"c:\ffmpeg\ffprobe.exe";
            proc.StartInfo.Arguments = cmd;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.UseShellExecute = false;
            if (!proc.Start())
            {
                Console.WriteLine("Error starting");
            }
            string outputString = proc.StandardOutput.ReadToEnd();
            string[] metaData = outputString.Trim().Split(new char[] { 'x', '\n' });
            // Remove the milliseconds
            MediaProperties properties = new MediaProperties()
            {
                Width = int.Parse(metaData[0]),
                Height = int.Parse(metaData[1]),
                Duration = TimeSpan.FromSeconds(double.Parse(metaData.Length > 3 ? metaData[3].Trim() : metaData[2].Trim()))
            };
            proc.WaitForExit();
            proc.Close();
            return properties;
        }
    }
}