using System.Linq;
using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenVid.Models.Home;
using OpenVid.Models.Shared;
using Search;

namespace OpenVid.Controllers
{
    public class HomeController : Controller
    {
        private Videos _repo;
        private PaginatedSearch _search;
        private IConfiguration _configuration;
        private UrlResolver _urlResolver;

        public HomeController(Videos repo, UrlResolver urlResolver, PaginatedSearch search, IConfiguration configuration)
        {
            _repo = repo;
            _search = search;
            _configuration = configuration;
            _urlResolver = urlResolver;
        }

        public IActionResult Index()
        {
            HomeViewModel viewModel = new HomeViewModel()
            {
                Videos = new VideoListViewModel() //FileBaseUrl/thumbnail/@(video.Md5).jpg
                {
                    Videos = _search.PaginatedQuery(string.Empty, 1, out var hasNext).Select(v => new VideoViewModel()
                    {
                        Id = v.Id,
                        Name = v.Name,
                        ThumbnailUrl = _urlResolver.GetThumbnailUrl(v),
                        Length = string.Format("{0:00}:{1:00}", (int)v.Length.TotalMinutes, v.Length.Seconds)
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
    }
}
