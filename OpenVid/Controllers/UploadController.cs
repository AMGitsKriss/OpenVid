using OpenVid.Models.Upload;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Database;
using Database.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaToolkit.Model;
using MediaToolkit;
using MediaToolkit.Options;
using TagLib;
using Microsoft.Extensions.Configuration;

namespace OpenVid.Controllers
{
    public class UploadController : Controller
    {
        private IWebHostEnvironment _hostingEnvironment;
        private Videos _repo;

        public UploadController(IWebHostEnvironment environment, Videos repo)
        {
            _hostingEnvironment = environment;
            _repo = repo;
        }

        public IActionResult Index()
        {
            return View(new UploadViewModel());
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Upload(IFormFile file, bool multipleFiles)
        {
            try
            {
                if (multipleFiles)
                {
                    return await MultiUpload(file);
                }
                else
                {
                    return await SingleUpload(file);
                }
            }
            catch (Exception ex)
            {
                return PartialView("_UploadError", ex.Message);
            }
        }

        [HttpPost]
        public IActionResult Update(UpdateFormViewModel viewModel)
        {
            Video toSave = _repo.GetVideo(viewModel.Md5);
            if (toSave == null)
                return RedirectToAction("Index");

            toSave.Name = viewModel.Name;
            var vid = _repo.SaveVideo(toSave);
            var tag = _repo.SaveTagsForVideo(toSave, _repo.DefineTags((viewModel.Tags?.Trim() ?? string.Empty).Split(new char[] { ' ', '\n' }).ToList()));

            return RedirectToAction("Index", "Play", new { md5 = toSave.Md5 });
        }

        private string GenerateHash(IFormFile file)
        {
            using (var md5 = MD5.Create())
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                byte[] hash = md5.ComputeHash(ms.ToArray());
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
        private MediaProperties GetMetadata(string location)
        {
            string basePath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;
            string cmd = $"-v error -select_streams v:0 -show_entries stream=width,height,duration -show_entries format=duration -of csv=s=x:p=0 {location}";
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
        public class MediaProperties
        {
            public TimeSpan Duration { get; set; }

            public int Width { get; set; }

            public int Height { get; set; }
        }

        private void SaveThumb(string videoPath, string thumbPath)
        {
            var cmd = $" -y -itsoffset -1 -i \"{videoPath}\" -vcodec mjpeg -vframes 1 -filter:v \"scale='-1:min(168\\,iw)', pad=w=300:h=168:x=(ow-iw)/2:y=(oh-ih)/2:color=black\" \"{thumbPath}\"";

            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = @"c:\ffmpeg\ffmpeg.exe",
                Arguments = cmd
            };

            var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
            process.WaitForExit(5000);
        }

        private async Task<IActionResult> SingleUpload(IFormFile file)
        {
            if (!file.ContentType.Contains("video") || file.Length == 0)
                return PartialView("_UploadError", "Invalid File");

            try
            {
                Video toSave = await SaveVideo(_repo, file);

                UpdateFormViewModel viewModel = new UpdateFormViewModel()
                {
                    Md5 = toSave.Md5,
                    Name = toSave.Name,
                    Extension = toSave.Extension,
                    Width = toSave.Width,
                    Height = toSave.Height,
                    Size = toSave.Size,
                    Tags = string.Join(" ", toSave.VideoTag.Select(x => x.Tag.Name))
                };

                return PartialView("_UpdateForm", viewModel);
            }
            catch (Exception ex)
            {
                return PartialView("_UploadError", ex.Message);
            }
        }

        private async Task<IActionResult> MultiUpload(IFormFile file)
        {
            UploadResultViewModel viewModel;

            if (!file.ContentType.Contains("video") || file.Length == 0)
                viewModel = new UploadResultViewModel() { Name = "Error" };

            Video toSave = await SaveVideo(_repo, file);

            if (toSave == null)
                viewModel = new UploadResultViewModel() { Name = "Video already exists!" };
            else
            {
                viewModel = new UploadResultViewModel()
                {
                    Name = toSave.Name,
                    MD5 = toSave.Md5
                };
            }
            return PartialView("_UploadResult", viewModel);
        }

        private async Task<Video> SaveVideo(Videos video, IFormFile file)
        {
            string hash = GenerateHash(file);
            Video toSave = video.GetVideo(hash);
            if (toSave == null)
            {
                string videoDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "video");
                string thumbDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "thumbnail");
                string ext = Path.GetExtension(file.FileName).Replace(".", "");
                string originalName = Path.GetFileNameWithoutExtension(file.FileName);
                string filePath = Path.Combine(videoDirectory, $"{hash}.{ext}");
                string thumbPath = Path.Combine(thumbDirectory, $"{hash}.jpg");
                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                SaveThumb(filePath, thumbPath);

                var meta = GetMetadata(filePath);
                toSave = new Video()
                {
                    Md5 = hash,
                    Name = originalName,
                    Extension = ext,
                    Width = meta.Width,
                    Height = meta.Height,
                    Length = meta.Duration,
                    Size = file.Length
                };
                toSave = video.SaveVideo(toSave);
            }
            else
            {
                toSave.Name = $"Already Exists! - {toSave.Name}";
            }
            return toSave;
        }
    }
}
