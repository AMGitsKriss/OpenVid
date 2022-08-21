using Microsoft.AspNetCore.Mvc;
using TagCache;

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
