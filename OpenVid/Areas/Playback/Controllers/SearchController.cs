using Microsoft.AspNetCore.Mvc;
using OpenVid.Areas.Playback.Models.Search;
using OpenVid.Extensions;
using OrionDashboard.Web.Attributes;
using System.Linq;
using VideoHandler;

namespace OpenVid.Areas.Playback.Controllers
{
    [RequireLogin]
    [Area("playback")]
    public class SearchController : OpenVidController
    {
        private IVideoManager _videoManager;
        private ISearchManager _search;

        public SearchController(IVideoManager videoManager, ISearchManager search)
        {
            _videoManager = videoManager;
            _search = search;
        }

        [Route("[controller]/{searchString}")]
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
