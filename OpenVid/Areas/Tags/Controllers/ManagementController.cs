using Database;
using Microsoft.AspNetCore.Mvc;
using OpenVid.Areas.Tags.Models.Management;
using System.Linq;
using VideoHandler;

namespace OpenVid.Areas.Tags.Controllers
{
    [Area("tags")]
    public class ManagementController : Controller
    {
        private readonly IVideoManager _videoManager;

        public ManagementController(IVideoManager videoManager)
        {
            _videoManager = videoManager;
        }
        public IActionResult Index()
        {
            var test = _videoManager.GetAllTags().ToList();
            var model = new TagManagementViewModel
            {
                Tags = _videoManager.GetAllTags().Select(t => new TagImplicationViewModel()
                {
                    Id = t.Id,
                    Name = t.Name,
                    Type = t.TypeNavigation.Name,
                    Implications = t.TagImplicationFrom.ToDictionary(k => k.To.Id, v => v.To.Name)
                })
                .OrderBy(t => t.Id)
                .ToList()
            };
            return View(model);
        }
    }
}
