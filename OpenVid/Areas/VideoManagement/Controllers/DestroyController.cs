using Microsoft.AspNetCore.Mvc;
using OpenVid.Extensions;
using OpenVid.Models;
using Serilog;
using System;
using VideoHandler;

namespace OpenVid.Areas.VideoManagement.Controllers
{

    [Area("videomanagement")]
    public class DestroyController : OpenVidController
    {
        private readonly IVideoManager _videoManager;
        private readonly ILogger _logger;

        public DestroyController(ILogger logger, IVideoManager videoManager)
        {
            _videoManager = videoManager;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Index(int id)
        {
            var video = _videoManager.GetVideo(id);
            try
            {
                _videoManager.HardDeleteVideo(id);
                _logger.Information("Successfully deleted {Id}:{Name}", video.Id, video.Name);
                return Json(new AjaxResponse());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Deletion error while deleting {Id}:{Name}", video.Id, video.Name);
                var response = new AjaxResponse()
                {
                    Message = ex.Message
                };
                return Json(response);
            }
        }
    }
}
