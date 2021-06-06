using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenVid.Models.Search;
using OpenVid.Models.Shared;
using Search;
using System.Linq;

namespace OpenVid.Controllers
{
    public class SearchController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private Videos _repo;
        private PaginatedSearch _search;

        public SearchController(ILogger<HomeController> logger, Videos repo, PaginatedSearch search)
        {
            _logger = logger;
            _repo = repo;
            _search = search;
        }

        [Route("[Controller]/{searchString}")]
        public IActionResult Index(string searchString)
        {
            SearchViewModel viewModel = new SearchViewModel();
            viewModel.Videos = new VideoListViewModel()
            {
                Videos = _search.PaginatedQuery(searchString, 1, out var hasNext),
                NextPageNumber = 2,
                HasNextPage = hasNext,
                SearchQuery = searchString
            };
            var videoIDs = viewModel.Videos.Videos.Select(x => x.Id).ToList();
            var tagIDs = _repo.GetAllVideos().Where(x => videoIDs.Contains(x.Id)).SelectMany(x => x.VideoTag).Select(x => x.TagId).ToList();
            viewModel.Tags = _repo.GetAllTags().Where(x => tagIDs.Contains(x.Id)).ToList();
            viewModel.SearchString = searchString;

            return View(viewModel);
        }

        public IActionResult Page(int pageNo, string searchString = "")
        {
            VideoListViewModel viewModel = new VideoListViewModel()
            {
                Videos = _search.PaginatedQuery(searchString ?? "", pageNo, out var hasMore),
                HasNextPage = hasMore,
                NextPageNumber = pageNo + 1,
                SearchQuery = searchString
            };
            return PartialView("_VideoList", viewModel);
        }
    }
}
