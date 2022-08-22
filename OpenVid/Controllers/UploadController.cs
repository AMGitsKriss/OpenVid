using OpenVid.Models.Upload;
using System;
using System.Linq;
using Database.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using VideoHandler;
using VideoHandler.Models;
using OpenVid.Extensions;

namespace OpenVid.Controllers
{
    public class UploadController : OpenVidController
    {
        private IVideoManager _videoManager;

        public UploadController(IVideoManager save)
        {
            _videoManager = save;
        }

        public IActionResult Index()
        {
            return View(new UploadViewModel());
        }

        [HttpPost]
        [RequestSizeLimit(10L * 1024L * 1024L * 1024L)]
        [RequestFormLimits(MultipartBodyLengthLimit = 10L * 1024L * 1024L * 1024L)]
        public async Task<IActionResult> Upload(IFormFile file, bool multipleFiles)
        {
            try
            {
                if (multipleFiles)
                {
                    return await MultiUpload(file);
                }
                else
                {
                    return await SingleUpload(file);
                }
            }
            catch (Exception ex)
            {
                return PartialView("_UploadError", ex.Message);
            }
        }

        [HttpPost]
        public IActionResult Update(UpdateFormViewModel viewModel)
        {
            Video toSave = _videoManager.GetVideo(viewModel.Id);
            if (toSave == null)
                return RedirectToAction("Index");

            toSave.Name = viewModel.Name;
            toSave.Description = viewModel.Description;
            toSave.RatingId = viewModel.RatingId == 0 ? null : viewModel.RatingId;

            _videoManager.SaveVideo(toSave);
            var tagList = _videoManager.DefineTags((viewModel.Tags?.Trim() ?? string.Empty).Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList());
            _videoManager.SaveTagsForVideo(toSave, tagList);

            return RedirectToAction(SiteMap.PlaybackPlay, new {  Id = toSave.Id });
            //return RedirectToAction("Index", "Play", new { Area = "Playback", Id = toSave.Id });
        }

        [HttpPost]
        public IActionResult Delete(UpdateFormViewModel viewModel)
        {
            _videoManager.SoftDelete(viewModel.Id);

            return RedirectToAction("Index", "Play", new { Id = viewModel.Id });
        }

        [HttpGet]
        public IActionResult UpdateAllMeta()
        {
            var allMds5 = _videoManager.GetVideos().OrderBy(v => v.Id).SelectMany(v => v.VideoSource.Select(s => s.Md5)).ToList();
            int count = 0;
            int total = allMds5.Count();
            foreach (var md5 in allMds5)
            {
                _videoManager.UpdateMeta(md5);
                count++;
            }
            return PartialView(200);
        }

        private async Task<IActionResult> SingleUpload(IFormFile file)
        {
            if (!file.ContentType.Contains("video") || file.Length == 0)
                return PartialView("_UploadError", "Invalid File");

            try
            {
                SaveVideoRequest request = new SaveVideoRequest()
                {
                    File = file
                };
                SaveVideoResponse response = await _videoManager.SaveVideoAsync(request);

                if(!string.IsNullOrWhiteSpace(response.Message))
                    return PartialView("_UploadError", response.Message);

                UpdateFormViewModel viewModel = new UpdateFormViewModel()
                {
                    Id = response.Video.Id,
                    Name = response.Video.Name,
                    Tags = string.Join(" ", response.Video.VideoTag.Select(x => x.Tag.Name)),
                    RatingId = response.Video.RatingId ?? 0,
                    PossibleRatings = _videoManager.GetRatings(),
                    SuggestedTags = new List<SuggestedTagViewModel>(),
                    Metadata = response.Video.VideoSource.Select(s => new MetadataViewModel()
                    {
                        Md5 = s.Md5,
                        Extension = s.Extension,
                        Width = s.Width,
                        Height = s.Height,
                        Size = s.Size,

                    }).ToList()
                };

                return PartialView("_UpdateForm", viewModel);
            }
            catch (Exception ex)
            {
                return PartialView("_UploadError", ex.Message);
            }
        }

        private async Task<IActionResult> MultiUpload(IFormFile file)
        {
            UploadResultViewModel viewModel;

            if (!file.ContentType.Contains("video") || file.Length == 0)
                viewModel = new UploadResultViewModel() { Name = "Error" };

            SaveVideoRequest request = new SaveVideoRequest()
            {
                File = file
            };
            SaveVideoResponse response = await _videoManager.SaveVideoAsync(request);

            if (response.AlreadyExists)
                viewModel = new UploadResultViewModel() { Name = "Video already exists!" };
            else
            {
                viewModel = new UploadResultViewModel()
                {
                    Name = response.Video.Name,
                    Id = response.Video.Id
                };
            }
            return PartialView("_UploadResult", viewModel);
        }
    }
}
