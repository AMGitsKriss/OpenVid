using System.Linq;
using Database;
using Microsoft.AspNetCore.Mvc;
using OpenVid.Models.Home;

namespace OpenVid.Controllers
{
    public class HomeController : Controller
    {
        private Videos _repo;

        public HomeController(Videos repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            HomeViewModel viewModel = new HomeViewModel()
            {
                Tags = _repo.GetAllTags().ToList()
            };

            return View(viewModel);
        }
    }
}
