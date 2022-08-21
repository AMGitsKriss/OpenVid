using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenVid.Models.Search;
using OpenVid.Models.Shared;
using Search;
using System.Linq;
using Upload;

namespace OpenVid.Controllers
{
    public class SearchController : Controller
    {
        private IVideoManager _videoManager;
        private PaginatedSearch _search;
        private IConfiguration _configuration;
        private UrlResolver _urlResolver;

        public SearchController(IVideoManager videoManager, UrlResolver urlResolver, PaginatedSearch search, IConfiguration configuration)
        {
            _videoManager = videoManager;
            _search = search;
            _configuration = configuration;
            _urlResolver = urlResolver;
        }

        [Route("[Controller]/{searchString}")]
        public IActionResult Index(string searchString)
        {
            SearchViewModel viewModel = new SearchViewModel();

            viewModel.SearchString = searchString;

            // TODO - Search results now? Nono remove
            var searchResults = _search.PaginatedQuery(searchString, 1, out var hasNext).Select(v => new VideoViewModel()
            {
                Id = v.Id,
                Name = v.Name,
                ThumbnailUrl = _urlResolver.GetThumbnailUrl(v),
                Length = string.Format("{0:00}:{1:00}", (int)v.Length.TotalMinutes, v.Length.Seconds)
            }).ToList();

            var videoIDs = searchResults.Select(x => x.Id).ToList();
            var selectedVideos = _videoManager.GetVideos().Where(x => videoIDs.Contains(x.Id));
            var tags = selectedVideos.SelectMany(x => x.VideoTag);
            var tagIDs = tags.Select(x => x.TagId).ToList();
            viewModel.Tags = _videoManager.GetAllTags().Where(x => tagIDs.Contains(x.Id)).ToList();

            return View(viewModel);
        }

        public IActionResult Page(int pageNo = 0, string searchString = "")
        {
            VideoListViewModel viewModel = new VideoListViewModel()
            {
                Videos = _search.PaginatedQuery(searchString ?? "", pageNo, out var hasMore).Select(v => new VideoViewModel()
                {
                    Id = v.Id,
                    Name = v.Name,
                    ThumbnailUrl = _urlResolver.GetThumbnailUrl(v),
                    Length = string.Format("{0:00}:{1:00}", (int)v.Length.TotalMinutes, v.Length.Seconds)
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
