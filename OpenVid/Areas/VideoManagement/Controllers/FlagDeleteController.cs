using Microsoft.AspNetCore.Mvc;
using OpenVid.Areas.Playback.Models.Update;
using OpenVid.Extensions;
using VideoHandler;

namespace OpenVid.Areas.VideoManagement.Controllers
{
    [Area("VideoManagement")]
    public class FlagDeleteController : OpenVidController
    {
        private IVideoManager _videoManager;

        public FlagDeleteController(IVideoManager videoManager)
        {
            _videoManager = videoManager;
        }

        [HttpPost]
        public IActionResult Index(UpdateFormViewModel viewModel)
        {
            _videoManager.SoftDelete(viewModel.Id);

            return RedirectToAction(SiteMap.Playback_Play, new { Id = viewModel.Id });
        }
    }
}
