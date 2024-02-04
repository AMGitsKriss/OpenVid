using Database.Users;
using Microsoft.AspNetCore.Mvc;
using OpenVid.Extensions;
using OrionDashboard.Web.Attributes;
using Serilog;
using VideoHandler;

namespace OpenVid.Areas.VideoManagement.Controllers
{
    [Area("videomanagement")]
    [RequirePermission(Permissions.Catalog_Delete)]
    public class FlagDeleteController : OpenVidController
    {
        private readonly IVideoManager _videoManager;
        private readonly ILogger _logger;

        public FlagDeleteController(ILogger logger, IVideoManager videoManager)
        {
            _videoManager = videoManager;
            _logger = logger;

        }

        [HttpPost]
        public IActionResult Index(int id)
        {
            if (_videoManager.SoftDelete(id))
            {
                _logger.Information($"User {User.Identity.Name} un/flagged {id} for deletion.");
            }

            return RedirectToAction(SiteMap.Playback_Play, new { Id = id });
        }
    }
}
