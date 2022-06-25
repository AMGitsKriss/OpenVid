using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenVid.Models.Play;
using OpenVid.Models.Upload;
using Search;
using System.Linq;
using Upload;

namespace OpenVid.Controllers
{
    public class PlayController : Controller
    {
        private Save _service;
        private UrlResolver _urlResolver;
        private IConfiguration _configuration;

        public PlayController(Save service, UrlResolver urlResolver, IConfiguration configuration)
        {
            _service = service;
            _urlResolver = urlResolver;
            _configuration = configuration;
        }

        [Route("[controller]/{md5}")]
        public IActionResult Index(string md5)
        {
            var video = _service.GetVideo(md5);

            PlayViewModel viewModel = new PlayViewModel()
            {
                VideoUrl = _urlResolver.GetVideoUrl(video),
                FileBaseUrl = _configuration["FileBaseUrl"]
            };

            if (string.IsNullOrWhiteSpace(viewModel.VideoUrl)) return NotFound();

            viewModel.Update = new UpdateFormViewModel()
            {
                Md5 = video.Md5,
                Name = video.Name,
                Extension = video.Extension,
                Width = video.Width,
                Height = video.Height,
                Size = video.Size,
                Description = video.Description,
                Meta = video.MetaText,
                Tags = string.Join(" ", video.VideoTag.Select(x => x.Tag.Name)),
                IsFlaggedForDeletion = video.IsDeleted,
                RatingId = video.RatingId ?? 0,
                PossibleRatings = _service.GetRatings(),
                FileBaseUrl = _configuration["FileBaseUrl"]
            };

            return View(viewModel);
        }

        public IActionResult GetTags()
        {
            var tags = _service.GetAllTags().Select(t => t.Name).ToList();

            return Json(tags);
        }
    }
}
