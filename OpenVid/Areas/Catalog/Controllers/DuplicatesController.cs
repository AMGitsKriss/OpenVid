using Microsoft.AspNetCore.Mvc;
using System.Linq;
using OpenVid.Models.Shared;
using VideoHandler;
using OpenVid.Extensions;

namespace OpenVid.Areas.Catalog.Controllers
{
    [Area("catalog")]
    public class DuplicatesController : OpenVidController
    {
        private readonly DuplicateFinder _duplicateFinder;
        private IUrlResolver _urlResolver;

        public DuplicatesController(DuplicateFinder duplicateFinder, IUrlResolver urlResolver)
        {
            _duplicateFinder = duplicateFinder;
            _urlResolver = urlResolver;
        }
        public IActionResult Index(bool rescan = false, int? minSecs = null, int? maxSecs = null)
        {
            //var results = _duplicateFinder.Get();
            if (rescan)
                _duplicateFinder.Update();

            var results = _duplicateFinder.Get(minSecs: minSecs, maxSecs: maxSecs).OrderBy(kv => kv.Value.TotalSeconds).ToDictionary(kv => kv.Key, kv => kv.Value.Videos.Select(
                v => new VideoViewModel
                {
                    Id = v.Id,
                    Name = v.Name,
                    Length = string.Format("{0:00}:{1:00}:{2:00}", v.Length.Hours, v.Length.Minutes, v.Length.Seconds)
                }
            ).ToList());

            return View(results);
        }
    }
}
