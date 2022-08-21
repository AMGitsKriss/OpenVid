using Database;
using Database.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Upload.Models;

namespace Upload
{
    public class Save : Videos
    {
        private IConfiguration _configuration;
        private IHostingEnvironment _hostingEnvironment;

        public Save(IHostingEnvironment environment, OpenVidContext context, IConfiguration configuration) : base(configuration, context)
        {
            _configuration = configuration;
            _hostingEnvironment = environment;
        }

        public async Task<SaveVideoResponse> SaveVideoAsync(SaveVideoRequest request)
        {
            string hash = GenerateHash(request.File);
            string subFolder = hash.Substring(0, 2);
            Video toSave = GetVideo(hash);
            string error = null;
            bool exists = toSave != null;
            if (!exists)
            {
                string videoDirectory = Path.Combine(_configuration["Urls:BucketDirectory"], "video", subFolder);
                string thumbDirectory = Path.Combine(_configuration["Urls:BucketDirectory"], "thumbnail", subFolder);

                TouchDirectory(videoDirectory);
                TouchDirectory(thumbDirectory);

                string ext = Path.GetExtension(request.File.FileName).Replace(".", "");
                string originalName = Path.GetFileNameWithoutExtension(request.File.FileName);
                string filePath = Path.Combine(videoDirectory, $"{hash}.{ext}");
                string thumbPath = Path.Combine(thumbDirectory, $"{hash}.jpg");
                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    try
                    {
                        await request.File.CopyToAsync(fileStream);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Failed to write file");
                    }
                }
                SaveThumb(filePath, thumbPath);

                var meta = GetMetadata(filePath);
                toSave = new Video()
                {
                    Name = originalName,
                    Length = meta.Duration,
                    VideoSource = new List<VideoSource>()
                    {
                        new VideoSource()
                        {
                            Md5 = hash,
                            Extension = ext,
                            Width = meta.Width,
                            Height = meta.Height,
                            Size = request.File.Length,
                        }
                    }
                };
                try
                {
                    toSave = SaveVideo(toSave);
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }
            }
            SaveVideoResponse response = new SaveVideoResponse()
            {
                Video = toSave,
                AlreadyExists = exists,
                Message = error
            };

            return response;
        }

        public void UpdateMeta(string md5)
        {
            //Video toSave = GetVideo(md5);
            VideoSource toSave = GetVideoSource(md5);

            string hash = toSave.Md5;
            string subFolder = hash.Substring(0, 2);
            string videoDirectory = Path.Combine(_configuration["Urls:BucketDirectory"], "video", subFolder);
            string filePath = Path.Combine(videoDirectory, $"{toSave.Md5}.{toSave.Extension}");
            var meta = GetMetadata(filePath);

            toSave.Width = meta.Width;
            toSave.Height = meta.Height;
            toSave.Video.Length = meta.Duration;
            toSave.Size = new FileInfo(filePath).Length;

            SaveVideoSource(toSave);

        }
        public async Task<SaveVideoResponse> ImportVideoAsync(ImportVideoRequest request)
        {
            string md5 = GenerateHash(request.FileNameFull);
            string subFolder = md5.Substring(0, 2);
            string ext = Path.GetExtension(request.FileName).Replace(".", "");
            bool videoExists = GetVideo(md5) != null;

            string videoDirectory = $"{_configuration["Urls:BucketDirectory"]}\\video\\{subFolder}\\";
            string thumbnailDirectory = $"{_configuration["Urls:BucketDirectory"]}\\thumbnail\\{subFolder}\\";

            if (videoExists)
                return new SaveVideoResponse()
                {
                    AlreadyExists = true,
                    Message = $"Video with the hash {md5} already exists. Skipping."
                };

            //THUMBNAIL
            string thumbPath = Path.Combine(thumbnailDirectory, $"{md5}.jpg");
            SaveThumb(request.FileNameFull, thumbPath);

            var metaData = GetMetadata(request.FileNameFull);
            FileInfo fileInfo = new FileInfo(request.FileNameFull);
            var newVideo = new Video()
            {
                Name = Path.GetFileNameWithoutExtension(request.FileName),
                Length = metaData.Duration,               
                VideoSource = new List<VideoSource>()
                    {
                        new VideoSource()
                        {
                            Md5 = md5,
                            Extension = ext,
                            Width = metaData.Width,
                            Height = metaData.Height,
                            Size = fileInfo.Length,
                        }
                    }
            };

            // DATABASE
            try
            {
                SaveVideo(newVideo);
            }
            catch (Exception ex)
            {
                return new SaveVideoResponse()
                {
                    AlreadyExists = false,
                    Message = $"Video named {request.FileName} couldn't be added to the database. Skipping."
                };
            }

            // FILE
            try
            {
                string destinationFileName = $"{videoDirectory}{md5}.{ext}";
                if (File.Exists(request.FileNameFull))
                {
                    if (!Directory.Exists(videoDirectory))
                        Directory.CreateDirectory(videoDirectory);
                    File.Move(request.FileNameFull, destinationFileName);
                }
            }
            catch (Exception ex)
            {
                return new SaveVideoResponse()
                {
                    AlreadyExists = false,
                    Message = $"Video named {request.FileName} Couldn't be copied to the bucket. Skipping."
                };
            }

            return new SaveVideoResponse()
            {
                Video = newVideo
            };
        }

        private string GenerateHash(object fileLocation)
        {
            throw new NotImplementedException();
        }

        private void TouchDirectory(string directory)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
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

        private string GenerateHash(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var ms = File.OpenRead(filename))
                {
                    byte[] hash = md5.ComputeHash(ms);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }

        }

        private void SaveThumb(string videoPath, string thumbPath)
        {
            var cmd = $" -y -itsoffset -1 -i \"{videoPath}\" -vcodec mjpeg -vframes 60 -filter:v \"scale='-1:min(168\\,iw)', pad=w=300:h=168:x=(ow-iw)/2:y=(oh-ih)/2:color=black\" \"{thumbPath}\"";

            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = @"c:\ffmpeg\ffmpeg.exe", // TODO - Make configurable
                Arguments = cmd
            };

            var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
            process.WaitForExit(5000);
        }

        private MediaProperties GetMetadata(string location)
        {
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
