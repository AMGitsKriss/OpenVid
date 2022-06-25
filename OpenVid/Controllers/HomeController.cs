using System.Diagnostics;
using System.Linq;
using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenVid.Models;
using OpenVid.Models.Home;
using OpenVid.Models.Shared;
using Search;

namespace OpenVid.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private Videos _repo;
        private PaginatedSearch _search;
        private IConfiguration _configuration;
        private UrlResolver _urlResolver;

        public HomeController(ILogger<HomeController> logger, Videos repo, UrlResolver urlResolver, PaginatedSearch search, IConfiguration configuration)
        {
            _logger = logger;
            _repo = repo;
            _search = search;
            _configuration = configuration;
            _urlResolver = urlResolver;
        }

        public IActionResult Index()
        {
            HomeViewModel viewModel = new HomeViewModel()
            {
                Videos = new Models.Shared.VideoListViewModel() //FileBaseUrl/thumbnail/@(video.Md5).jpg
                {
                    Videos = _search.PaginatedQuery(string.Empty, 1, out var hasNext).Select(v => new VideoViewModel()
                    {
                        Id = v.Id,
                        Name = v.Name,
                        Md5=v. Md5, 
                        ThumbnailUrl = _urlResolver.GetThumbnailUrl(v)
                    }).ToList(),
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
