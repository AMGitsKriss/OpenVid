using System.Linq;
using Microsoft.AspNetCore.Mvc;
using OpenVid.Models.Home;
using OrionDashboard.Web.Attributes;
using VideoHandler;

namespace OpenVid.Controllers
{
    [RequireLogin]
    public class HomeController : Controller
    {
        private IVideoManager _manager;

        public HomeController(IVideoManager manager)
        {
            _manager = manager;
        }

        public IActionResult Index()
        {
            HomeViewModel viewModel = new HomeViewModel()
            {
                Tags = _manager.GetAllTags().ToList()
            };

            return View(viewModel);
        }
    }
}
