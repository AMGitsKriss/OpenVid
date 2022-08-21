using Microsoft.AspNetCore.Mvc;
using OpenVid.Models.Search;
using Search;
using System.Linq;
using Upload;

namespace OpenVid.Controllers
{
    public class SearchController : Controller
    {
        private IVideoManager _videoManager;
        private PaginatedSearch _search;
        private UrlResolver _urlResolver;

        public SearchController(IVideoManager videoManager, UrlResolver urlResolver, PaginatedSearch search)
        {
            _videoManager = videoManager;
            _search = search;
            _urlResolver = urlResolver;
        }

        [Route("[Controller]/{searchString}")]
        public IActionResult Index(string searchString)
        {
            SearchViewModel viewModel = new SearchViewModel();

            viewModel.SearchString = searchString;

            // TODO - Search results now? Nono. remove
            var videoIDs = _search.PaginatedQuery(searchString, 1, out var hasNext).Select(v => v.Id).ToList();

            var selectedVideos = _videoManager.GetVideos().Where(x => videoIDs.Contains(x.Id));
            var tags = selectedVideos.SelectMany(x => x.VideoTag);
            var tagIDs = tags.Select(x => x.TagId).ToList();
            viewModel.Tags = _videoManager.GetAllTags().Where(x => tagIDs.Contains(x.Id)).ToList();

            return View(viewModel);
        }
    }
}
