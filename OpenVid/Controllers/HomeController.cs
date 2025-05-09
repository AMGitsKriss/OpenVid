using System.Linq;
using Microsoft.AspNetCore.Mvc;
using OpenVid.Areas.Playback.Models.Search;
using OrionDashboard.Web.Attributes;
using Serilog;
using VideoHandler;

namespace OpenVid.Controllers
{
    [RequireLogin]
    public class HomeController : Controller
    {
        private readonly IVideoManager _manager;
        private readonly ILogger _logger;

        public HomeController(IVideoManager manager, ILogger logger)
        {
            _manager = manager;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var allTags = _manager.GetAllTags().GroupBy(t => t.Type).OrderBy(t => (t.Key ?? 0) );
            SearchViewModel viewModel = new SearchViewModel()
            {
                TagGroups = allTags.Select(t => new Models.Shared.TagViewModel() { 
                    Category = t.FirstOrDefault()?.TypeNavigation?.Name ?? "Tags",
                    Tags = t.ToList()
                }).ToList()
            };

            return View(viewModel);
        }
    }
}
