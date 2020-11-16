using System.Diagnostics;
using System.Linq;
using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenVid.Models;
using OpenVid.Models.Home;

namespace OpenVid.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private Videos _repo;

        public HomeController(ILogger<HomeController> logger, Videos repo)
        {
            _logger = logger;
            _repo = repo;
        }

        public IActionResult Index()
        {
            HomeViewModel viewModel = new HomeViewModel()
            {
                Videos = _repo.GetAllVideos().Take(100).ToList(),
                Tags = _repo.GetAllTags().ToList()
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
