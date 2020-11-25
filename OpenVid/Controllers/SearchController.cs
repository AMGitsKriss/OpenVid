using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenVid.Models.Search;
using System.Linq;

namespace OpenVid.Controllers
{
    public class SearchController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private Videos _repo;
        private Search.Search _search;

        public SearchController(ILogger<HomeController> logger, Videos repo, Search.Search search)
        {
            _logger = logger;
            _repo = repo;
            _search = search;
        }

        [Route("[Controller]/{searchString}")]
        public IActionResult Index(string searchString)
        {
            SearchViewModel viewModel = new SearchViewModel();
            viewModel.Videos = _search.Query(searchString);
            var videoIDs = viewModel.Videos.Select(x => x.Id).ToList();
            var tagIDs = _repo.GetAllVideos().Where(x => videoIDs.Contains(x.Id)).SelectMany(x => x.VideoTag).Select(x => x.TagId).ToList();
            viewModel.Tags = _repo.GetAllTags().Where(x => tagIDs.Contains(x.Id)).ToList();
            viewModel.SearchString = searchString;

            return View(viewModel);
        }
    }
}
