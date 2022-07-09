using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenVid.Models.Curation;
using OpenVid.Models.Shared;
using Search;
using System.Linq;

namespace OpenVid.Controllers
{
    public class CurationController : Controller
    {
        private Videos _repo;
        private IConfiguration _configuration;
        private UrlResolver _urlResolver;

        public CurationController(Videos repo, UrlResolver urlResolver, IConfiguration configuration)
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
                    Md5 = v.Md5,
                    SizeMb = (int)((v.Size / 1024) / 1024),
                    Length = v.Length.ToString(),
                    ThumbnailUrl = _urlResolver.GetThumbnailUrl(v)
                }).ToList()
            };

            return View(model);
        }
    }
}
