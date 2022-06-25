using Database.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Search
{
    public class UrlResolver
    {
        private IConfiguration _configuration;

        public UrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetVideoUrl(Video video)
        {
            var bucketDirectory = $"{_configuration["Urls:BucketDirectory"]}\\video\\{video.Md5.Substring(0, 2)}\\";
            var internalDirectory = $"{_configuration["Urls:InternalDirectory"]}\\video\\";
            var fileName = $"{video.Md5}.{video.Extension}";

            if (!File.Exists(bucketDirectory + fileName))
            {
                if (!TryMove(internalDirectory, bucketDirectory, fileName))
                    return $"{_configuration["Urls:InternalUrl"]}/video/{video.Md5}.{video.Extension}";
            }

            return $"{_configuration["Urls:BucketUrl"]}/video/{video.Md5.Substring(0, 2)}/{video.Md5}.{video.Extension}";
        }

        public string GetThumbnailUrl(Video video)
        {
            var bucketDirectory = $"{_configuration["Urls:BucketDirectory"]}\\thumbnail\\{video.Md5.Substring(0, 2)}\\";
            var internalDirectory = $"{_configuration["Urls:InternalDirectory"]}\\thumbnail\\";
            var fileName = $"{video.Md5}.jpg";

            if (!File.Exists(bucketDirectory + fileName))
            {
                if(!TryMove(internalDirectory, bucketDirectory, fileName))
                    return $"{_configuration["Urls:InternalUrl"]}/thumbnail/{video.Md5}.jpg";
            }

            return $"{_configuration["Urls:BucketUrl"]}/thumbnail/{video.Md5.Substring(0, 2)}/{video.Md5}.jpg";
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
