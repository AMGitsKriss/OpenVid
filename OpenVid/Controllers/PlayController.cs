using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Search;
using TagCache;
using Upload;

namespace OpenVid.Controllers
{
    public class PlayController : Controller
    {
        private TagManager _tagManager;

        public PlayController(TagManager tagManager)
        {
            _tagManager = tagManager;
        }

        public IActionResult GetTags()
        {
            var tags = _tagManager.GetAllUsedTags();

            return Json(tags);
        }
    }
}
