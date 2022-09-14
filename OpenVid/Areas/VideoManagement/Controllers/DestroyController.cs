using Microsoft.AspNetCore.Mvc;
using OpenVid.Extensions;
using System;
using VideoHandler;

namespace OpenVid.Areas.VideoManagement.Controllers 
{ 

    [Area("videomanagement")]
    public class DestroyController : OpenVidController
    {
        private IVideoManager _videoManager;

        public DestroyController(IVideoManager videoManager)
        {
            _videoManager = videoManager;
        }

        [HttpPost]
        public IActionResult Index(int id)
        {
            try
            {
                _videoManager.HardDeleteVideo(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
    }
}
