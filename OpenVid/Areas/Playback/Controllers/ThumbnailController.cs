using CatalogManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IO;

namespace OpenVid.Areas.Playback.Controllers
{
    [Area("playback")]
    public class ThumbnailController : Controller
    {
        private readonly CatalogImportOptions _configuration;
        private readonly PlaybackService _playbackService;

        public ThumbnailController(IOptions<CatalogImportOptions> configuration, PlaybackService playbackService)
        {
            _configuration = configuration.Value;
            _playbackService = playbackService;
        }

        [Route("[area]/[controller]/{id}")]
        public IActionResult Index([FromRoute]int id)
        {
            var idString = id.ToString().PadLeft(2, '0');
            var thumbnailFolder = Path.Combine(_configuration.BucketDirectory, "thumbnail", idString.Substring(0, 2));
            var thumbnailFile = $"{idString}.jpg";
            var fileFullName = Path.Combine(thumbnailFolder, thumbnailFile);

            // Try once to make it
            if (!System.IO.File.Exists(fileFullName))
            {
                _playbackService.TryGenerateThumbnail(id);
            }

            // Otherwise just return the placeholder card
            if (!System.IO.File.Exists(fileFullName))
            {
                return File(System.IO.File.ReadAllBytes("wwwroot/img/thumb.png"), "image/png");
            }

            return File(System.IO.File.ReadAllBytes(Path.Combine(thumbnailFolder, thumbnailFile)), "image/jpeg");
        }
    }
}
