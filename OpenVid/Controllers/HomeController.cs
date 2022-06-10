using System.Diagnostics;
using System.Linq;
using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenVid.Models;
using OpenVid.Models.Home;
using Search;

namespace OpenVid.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private Videos _repo;
        private PaginatedSearch _search;
        private IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, Videos repo, PaginatedSearch search, IConfiguration configuration)
        {
            _logger = logger;
            _repo = repo;
            _search = search;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            HomeViewModel viewModel = new HomeViewModel()
            {
                Videos = new Models.Shared.VideoListViewModel()
                {
                    Videos = _search.PaginatedQuery(string.Empty, 1, out var hasNext),
                    NextPageNumber = 2,
                    SearchQuery = string.Empty,
                    HasNextPage = hasNext,
                    FileBaseUrl = _configuration["FileBaseUrl"]
                },
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
