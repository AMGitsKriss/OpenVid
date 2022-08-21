using Database;
using Database.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;

namespace Upload
{
    public class Delete : Videos
    {
        private IConfiguration _configuration;
        private IHostingEnvironment _hostingEnvironment;

        public Delete(IHostingEnvironment environment, OpenVidContext context, IConfiguration configuration) : base(configuration, context)
        {
            _configuration = configuration;
            _hostingEnvironment = environment;
        }

        public bool DeleteVideo(int id)
        {
            Video video = GetVideo(id);

            foreach (var source in video.VideoSource.ToList())
            {
                var bucketDirectory = $"{_configuration["Urls:BucketDirectory"]}\\video\\{source.Md5.Substring(0, 2)}\\";
                var internalDirectory = $"{_configuration["Urls:InternalDirectory"]}\\video\\";
                var fileName = $"{source.Md5}.{source.Extension}";

                if (File.Exists(bucketDirectory + fileName))
                {
                    File.Delete(bucketDirectory + fileName);
                }
                else if (File.Exists(internalDirectory + fileName))
                {
                    File.Delete(internalDirectory + fileName);
                }

                DeleteSource(source.Md5);
            }

            video = GetVideo(id);
            if (!video.VideoSource.Any())
            {
                base.DeleteVideo(id);
            }

            return true;
        }
    }
}
