using Microsoft.AspNetCore.Mvc;
using TagCache;

namespace OpenVid.Areas.Playback.Controllers
{
    [Area("playback")]
    public class TagController : Controller
    {
        private TagManager _tagManager;

        public TagController(TagManager tagManager)
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
