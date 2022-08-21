using Microsoft.AspNetCore.Mvc;
using OpenVid.Models.Curation;
using OpenVid.Models.Shared;
using Search;
using System;
using System.Linq;
using Upload;

namespace OpenVid.Controllers
{
    public class CurationController : Controller
    {
        private IVideoManager _videoManager;
        private UrlResolver _urlResolver;

        public CurationController(IVideoManager videoManager, UrlResolver urlResolver)
        {
            _videoManager = videoManager;
            _urlResolver = urlResolver;
        }
        public IActionResult Index()
        {
            var model = new CurationViewModel()
            {
                VideosForDeletion = _videoManager.GetSoftDeletedVideos().Select(v => new VideoViewModel()
                {
                    Id = v.Id,
                    Name = v.Name,
                    SizeMb = (int)((v.VideoSource.First().Size / 1024) / 1024),
                    Length = v.Length.ToString(),
                    ThumbnailUrl = _urlResolver.GetThumbnailUrl(v)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(int id)
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
