using Database.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VideoHandler
{
    public class UrlResolver : IUrlResolver
    {
        private IConfiguration _configuration;

        public UrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Dictionary<string, string> GetVideoUrls(Video video)
        {
            var sources = new Dictionary<string, string>();

            foreach (var src in video.VideoSource)
            {
                // TODO - REFACTOR: Use Configuration Options here
                var bucketDirectory = $"{_configuration["Urls:BucketDirectory"]}\\video\\{src.Md5.Substring(0, 2)}\\";
                var internalDirectory = $"{_configuration["Urls:InternalDirectory"]}\\video\\";
                var fileName = $"{src.Md5}.{src.Extension}";

                if (src.Extension == "mp4")
                    sources.Add(src.Extension, $"{_configuration["Urls:BucketUrl"]}/video/{src.Md5.Substring(0, 2)}/{src.Md5}.{src.Extension}");
                else if (src.Extension == "mpd")
                    sources.Add(src.Extension, $"{_configuration["Urls:BucketUrl"]}/video/{src.Md5.Substring(0, 2)}/{src.Md5}/dash.{src.Extension}");
                else if (src.Extension == "m3u8")
                    sources.Add(src.Extension, $"{_configuration["Urls:BucketUrl"]}/video/{src.Md5.Substring(0, 2)}/{src.Md5}/hls.{src.Extension}");
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
