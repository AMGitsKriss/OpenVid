using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenVid.Models.Play;
using OpenVid.Models.Upload;
using System.Linq;

namespace OpenVid.Controllers
{
    public class PlayController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private Videos _repo;
        private IConfiguration _configuration;

        public PlayController(ILogger<HomeController> logger, Videos repo, IConfiguration configuration)
        {
            _logger = logger;
            _repo = repo;
            _configuration = configuration;
        }

        [Route("[controller]/{md5}")]
        public IActionResult Index(string md5)
        {
            PlayViewModel viewModel = new PlayViewModel()
            {
                Video = _repo.GetVideo(md5),
                FileBaseUrl = _configuration["FileBaseUrl"]
            };

            if (viewModel.Video == null) return NotFound();

            viewModel.Update = new UpdateFormViewModel()
            {
                Md5 = viewModel.Video.Md5,
                Name = viewModel.Video.Name,
                Extension = viewModel.Video.Extension,
                Width = viewModel.Video.Width,
                Height = viewModel.Video.Height,
                Size = viewModel.Video.Size,
                Description = viewModel.Video.Description,
                Meta = viewModel.Video.MetaText,
                Tags = string.Join(" ", viewModel.Video.VideoTag.Select(x => x.Tag.Name)),
                IsFlaggedForDeletion = viewModel.Video.IsDeleted,
                RatingId = viewModel.Video.RatingId ?? 0,
                PossibleRatings = _repo.GetRatings(),
                FileBaseUrl = _configuration["FileBaseUrl"]
            };

            return View(viewModel);
        }
    }
}
