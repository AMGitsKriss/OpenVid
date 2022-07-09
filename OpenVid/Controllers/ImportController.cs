using Database;
using Microsoft.AspNetCore.Mvc;
using OpenVid.Models.Import;
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
        private Save _save;
        private static List<FoundVideoViewModel> PendingImports;

        public ImportController(Save save)
        {
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
                        FileLocation = info.DirectoryName,
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
            var fileInfo = PendingImports.Single(f => f.FileName == fileName);

            ImportVideoRequest request = new ImportVideoRequest()
            {
                FileName = fileInfo.FileName,
                FileLocation = fileInfo.FileLocation
            };

            SaveVideoResponse response = await _save.ImportVideoAsync(request);
            _save.SaveTagsForVideo(response.Video, _save.DefineTags(fileInfo.SuggestedTags));

            if (response.AlreadyExists)
                return BadRequest("Already Exists");
            else if (response.Video == null || response.Video.Id == 0)
                return BadRequest("Failed DB Insert: " + response.Message);
            else
            {
                return Ok("Done");
            }
        }
    }
}
