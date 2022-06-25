using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenVid.Models.Search;
using OpenVid.Models.Shared;
using Search;
using System;
using System.Linq;

namespace OpenVid.Controllers
{
    public class SearchController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private Videos _repo;
        private PaginatedSearch _search;
        private IConfiguration _configuration;
        private UrlResolver _urlResolver;

        public SearchController(ILogger<HomeController> logger, Videos repo, UrlResolver urlResolver, PaginatedSearch search, IConfiguration configuration)
        {
            _logger = logger;
            _repo = repo;
            _search = search;
            _configuration = configuration;
            _urlResolver = urlResolver;
        }

        [Route("[Controller]/{searchString}")]
        public IActionResult Index(string searchString)
        {
            SearchViewModel viewModel = new SearchViewModel();
            viewModel.Videos = new VideoListViewModel()
            {
                Videos = _search.PaginatedQuery(searchString, 1, out var hasNext).Select(v => new VideoViewModel()
                {
                    Id = v.Id,
                    Name = v.Name,
                    Md5 = v.Md5,
                    ThumbnailUrl = _urlResolver.GetThumbnailUrl(v)
                }).ToList(),
                NextPageNumber = 2,
                HasNextPage = hasNext,
                SearchQuery = searchString,
                FileBaseUrl = _configuration["FileBaseUrl"]
            };
            var videoIDs = viewModel.Videos.Videos.Select(x => x.Id).ToList();
            var selectedVideos = _repo.GetAllVideos().Where(x => videoIDs.Contains(x.Id));
            var tags = selectedVideos.SelectMany(x => x.VideoTag);
            var tagIDs = tags.Select(x => x.TagId).ToList();
            viewModel.Tags = _repo.GetAllTags().Where(x => tagIDs.Contains(x.Id)).ToList();
            viewModel.SearchString = searchString;

            return View(viewModel);
        }

        public IActionResult Page(int pageNo, string searchString = "")
        {
            VideoListViewModel viewModel = new VideoListViewModel()
            {
                Videos = _search.PaginatedQuery(searchString ?? "", pageNo, out var hasMore).Select(v => new VideoViewModel()
                {
                    Id = v.Id,
                    Name = v.Name,
                    Md5 = v.Md5,
                    ThumbnailUrl = _urlResolver.GetThumbnailUrl(v)
                }).ToList(),
                HasNextPage = hasMore,
                NextPageNumber = pageNo + 1,
                SearchQuery = searchString,
                FileBaseUrl = _configuration["FileBaseUrl"]
            };
            return PartialView("_VideoList", viewModel);
        }
    }
}
