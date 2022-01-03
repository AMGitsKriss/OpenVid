using OpenVid.Models.Upload;
using System;
using System.Linq;
using Database;
using Database.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Upload;
using Upload.Models;

namespace OpenVid.Controllers
{
    public class UploadController : Controller
    {
        private Videos _repo;
        private Save _save;

        public UploadController(Videos repo, Save save)
        {
            _repo = repo;
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
            Video toSave = _repo.GetVideo(viewModel.Md5);
            if (toSave == null)
                return RedirectToAction("Index");

            toSave.Name = viewModel.Name;
            toSave.MetaText = viewModel.Meta;
            toSave.Description = viewModel.Description;
            toSave.RatingId = viewModel.RatingId == 0 ? null : viewModel.RatingId;
            var vid = _repo.SaveVideo(toSave);
            var tag = _repo.SaveTagsForVideo(toSave, _repo.DefineTags((viewModel.Tags?.Trim() ?? string.Empty).Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList()));

            return RedirectToAction("Index", "Play", new { md5 = toSave.Md5 });
        }

        [HttpPost]
        public IActionResult Delete(UpdateFormViewModel viewModel)
        {
            _repo.DeleteVideo(viewModel.Md5);

            return RedirectToAction("Index", "Play", new { md5 = viewModel.Md5 });
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

                UpdateFormViewModel viewModel = new UpdateFormViewModel()
                {
                    Md5 = response.Video.Md5,
                    Name = response.Video.Name,
                    Extension = response.Video.Extension,
                    Width = response.Video.Width,
                    Height = response.Video.Height,
                    Size = response.Video.Size,
                    Tags = string.Join(" ", response.Video.VideoTag.Select(x => x.Tag.Name)),
                    RatingId = response.Video.RatingId ?? 0,
                    PossibleRatings = _repo.GetRatings()
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
                    MD5 = response.Video.Md5
                };
            }
            return PartialView("_UploadResult", viewModel);
        }
    }
}
