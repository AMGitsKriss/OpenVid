using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenVid.Areas.Playback.Models.VideoList;
using OpenVid.Models.Shared;
using System.Linq;
using VideoHandler;

namespace OpenVid.Areas.Playback.Controllers
{
    [Area("Playback")]
    public class VideoListController : Controller
    {
        private readonly ISearchManager _search;
        private readonly IConfiguration _configuration;
        private readonly IUrlResolver _urlResolver;

        public VideoListController(IUrlResolver urlResolver, ISearchManager search, IConfiguration configuration)
        {
            _search = search;
            _configuration = configuration;
            _urlResolver = urlResolver;
        }

        public IActionResult Index(int pageNo = 0, string searchString = "")
        {
            VideoListViewModel viewModel = new VideoListViewModel()
            {
                Videos = _search.PaginatedQuery(searchString ?? "", pageNo, out var hasMore).Select(v => new VideoViewModel()
                {
                    Id = v.Id,
                    Name = v.Name,
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
