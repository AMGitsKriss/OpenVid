using System.Linq;
using Microsoft.AspNetCore.Mvc;
using OpenVid.Models.Home;
using VideoHandler;

namespace OpenVid.Controllers
{
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

        /*[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }*/
    }
}
