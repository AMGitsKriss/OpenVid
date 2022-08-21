using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        private Delete _repo;
        private IConfiguration _configuration;
        private UrlResolver _urlResolver;

        public CurationController(Delete repo, UrlResolver urlResolver, IConfiguration configuration)
        {
            _repo = repo;
            _configuration = configuration;
            _urlResolver = urlResolver;
        }
        public IActionResult Index()
        {
            var model = new CurationViewModel()
            {
                VideosForDeletion = _repo.GetSoftDeletedVideos().Select(v => new VideoViewModel()
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
                _repo.DeleteVideo(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
    }
}
