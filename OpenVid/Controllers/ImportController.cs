using Database;
using Database.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenVid.Models.Import;
using OpenVid.Models.Upload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Upload;
using Upload.Models;

namespace OpenVid.Controllers
{
    public class ImportController : Controller
    {
        private Videos _repo;
        private Save _save;
        private static List<FoundVideoViewModel> PendingImports;

        public ImportController(Videos repo, Save save)
        {
            _repo = repo;
            _save = save;
        }
        public IActionResult Index()
        {
            var importDir = $@"{Directory.GetCurrentDirectory()}\wwwroot\import_queue";
            Directory.CreateDirectory(importDir);
            PendingImports = FindFiles(importDir, importDir);

            var viewModel = new ImportViewModel()
            {
                DiscoveredFiles = PendingImports
            };

            return View(viewModel);
        }

        private List<FoundVideoViewModel> FindFiles(string dir, string prefix)
        {
            try
            {
                var result = new List<FoundVideoViewModel>();

                var suggestedTags = dir.Replace(prefix, "").Split(@"\", StringSplitOptions.RemoveEmptyEntries).ToList();

                // Files
                foreach (var file in Directory.GetFiles(dir))
                {
                    var info = new FileInfo(file);
                    var video = new FoundVideoViewModel()
                    {
                        FileName = info.Name,
                        FullName = info.FullName,
                        SuggestedTags = suggestedTags
                    };
                    result.Add(video);
                }

                // Directories
                foreach (var folder in Directory.GetDirectories(dir))
                {
                    result.AddRange(FindFiles(folder, prefix));
                }

                return result;
            }
            catch (DirectoryNotFoundException)
            {

                return FindFiles(dir, prefix);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Save(string fileName)
        {
            try
            {
                var fileInfo = PendingImports.Single(f => f.FileName == fileName);

                using var stream = new MemoryStream(System.IO.File.ReadAllBytes(fileInfo.FullName).ToArray());
                var formFile = new FormFile(stream, 0, stream.Length, "streamFile", fileInfo.FileName);

                return await Import(fileInfo, formFile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        private async Task<IActionResult> Import(FoundVideoViewModel fileInfo, IFormFile file)
        {
            if (file.Length == 0)
                return BadRequest("No File Data");

            SaveVideoRequest request = new SaveVideoRequest()
            {
                File = file
            };
            SaveVideoResponse response = await _save.SaveVideoAsync(request);
            _repo.SaveTagsForVideo(response.Video, _repo.DefineTags(fileInfo.SuggestedTags));

            System.IO.File.Delete(fileInfo.FullName);

            if (response.AlreadyExists)
                return BadRequest("Already Exists");
            else
            {
                return Ok("Done");
            }
            
        }
    }
}
