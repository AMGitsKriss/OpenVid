using Database.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace VideoHandler
{
    public class UrlResolver : IUrlResolver
    {
        private IConfiguration _configuration;

        public UrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<string> GetVideoUrl(Video video)
        {
            var sources = new List<string>();

            foreach (var src in video.VideoSource)
            {
                var bucketDirectory = $"{_configuration["Urls:BucketDirectory"]}\\video\\{src.Md5.Substring(0, 2)}\\";
                var internalDirectory = $"{_configuration["Urls:InternalDirectory"]}\\video\\";
                var fileName = $"{src.Md5}.{src.Extension}";

                if (!File.Exists(bucketDirectory + fileName))
                {
                    if (!TryMove(internalDirectory, bucketDirectory, fileName))
                        sources.Add($"{_configuration["Urls:InternalUrl"]}/video/{src.Md5}.{src.Extension}");
                }

                sources.Add($"{_configuration["Urls:BucketUrl"]}/video/{src.Md5.Substring(0, 2)}/{src.Md5}.{src.Extension}");
            }
            return sources;
        }

        private bool TryMove(string internalDirectory, string bucketDirectory, string fileName)
        {
            try
            {
                if (File.Exists(internalDirectory + fileName))
                {
                    if (!Directory.Exists(bucketDirectory))
                        Directory.CreateDirectory(bucketDirectory);
                    File.Move(internalDirectory + fileName, bucketDirectory + fileName);
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
