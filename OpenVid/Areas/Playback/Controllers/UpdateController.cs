using System;
using System.Linq;
using Database.Models;
using Microsoft.AspNetCore.Mvc;
using OpenVid.Areas.Playback.Models.Update;
using OpenVid.Extensions;
using OrionDashboard.Web.Attributes;
using VideoHandler;

namespace OpenVid.Areas.Playback.Controllers
{
    [RequireLogin]
    [Area("playback")]
    public class UpdateController : OpenVidController
    {
        private IVideoManager _videoManager;

        public UpdateController(IVideoManager save)
        {
            _videoManager = save;
        }

        [HttpPost]
        public IActionResult Index(UpdateFormViewModel viewModel)
        {
            Video toSave = _videoManager.GetVideo(viewModel.Id); // TODO - This is a Database reference and shouldn't be here.
            if (toSave == null)
                return RedirectToAction("Index");

            toSave.Name = viewModel.Name;
            toSave.Description = viewModel.Description;
            toSave.RatingId = viewModel.RatingId == 0 ? null : viewModel.RatingId;

            _videoManager.SaveVideo(toSave);
            var tagList = _videoManager.DefineTags((viewModel.Tags?.Trim() ?? string.Empty).Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList());
            _videoManager.SaveTagsForVideo(toSave, tagList);

            return RedirectToAction(SiteMap.Playback_Play, new { Id = toSave.Id });
        }
    }
}
