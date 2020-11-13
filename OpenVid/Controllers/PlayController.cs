using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenVid.Models.Play;
using OpenVid.Models.Upload;
using System.Linq;

namespace OpenVid.Controllers
{
    public class PlayController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public PlayController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Route("[controller]/{md5}")]
        public IActionResult Index(string md5)
        {
            Videos video = new Videos();

            PlayViewModel viewModel = new PlayViewModel()
            {
                Video = video.GetVideo(md5)
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
                Tags = string.Join(" ", viewModel.Video.VideoTag.Select(x => x.Tag.Name))
            };

            return View(viewModel);
        }
    }
}
