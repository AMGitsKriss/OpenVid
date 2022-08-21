using OpenVid.Models.Upload;
using System;
using System.Linq;
using Database.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Upload;
using Upload.Models;
using System.Collections.Generic;
using System.Threading;

namespace OpenVid.Controllers
{
    public class UploadController : Controller
    {
        private Save _save;

        public UploadController(Save save)
        {
            _save = save;
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
            Video toSave = _save.GetVideo(viewModel.Id);
            if (toSave == null)
                return RedirectToAction("Index");

            toSave.Name = viewModel.Name;
            toSave.Description = viewModel.Description;
            toSave.RatingId = viewModel.RatingId == 0 ? null : viewModel.RatingId;

            _save.SaveVideo(toSave);
            var tagList = _save.DefineTags((viewModel.Tags?.Trim() ?? string.Empty).Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList());
            _save.SaveTagsForVideo(toSave, tagList);

            return RedirectToAction("Index", "Play", new { Id = toSave.Id });
        }

        [HttpPost]
        public IActionResult Delete(UpdateFormViewModel viewModel)
        {
            _save.SoftDelete(viewModel.Id);

            return RedirectToAction("Index", "Play", new { Id = viewModel.Id });
        }

        [HttpGet]
        public IActionResult UpdateAllMeta()
        {
            var allMds5 = _save.GetAllVideos().OrderBy(v => v.Id).SelectMany(v => v.VideoSource.Select(s => s.Md5)).ToList();
            int count = 0;
            int total = allMds5.Count();
            foreach (var md5 in allMds5)
            {
                _save.UpdateMeta(md5);
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
                SaveVideoResponse response = await _save.SaveVideoAsync(request);

                if(!string.IsNullOrWhiteSpace(response.Message))
                    return PartialView("_UploadError", response.Message);

                UpdateFormViewModel viewModel = new UpdateFormViewModel()
                {
                    Id = response.Video.Id,
                    Name = response.Video.Name,
                    Tags = string.Join(" ", response.Video.VideoTag.Select(x => x.Tag.Name)),
                    RatingId = response.Video.RatingId ?? 0,
                    PossibleRatings = _save.GetRatings(),
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
            SaveVideoResponse response = await _save.SaveVideoAsync(request);

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
