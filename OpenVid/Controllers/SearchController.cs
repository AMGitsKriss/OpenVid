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
            SearchViewModel viewModel = new SearchViewModel()
            {
                //Videos = video.Search(searchString),
                Videos = _search.Query(searchString),
                Tags = _repo.GetAllTags().ToList(),
                SearchString = searchString
            };

            return View(viewModel);
        }
    }
}
