using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CatalogManager;
using CatalogManager.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenVid.Areas.Catalog.Models.Import;

namespace OpenVid.Areas.Catalog.Controllers
{
    [Area("catalog")]
    public class ImportController : Controller
    {
        private readonly ImportService _importService;

        public ImportController(ImportService importService)
        {
            _importService = importService; ;
        }

        public IActionResult Index()
        {
            var model = new ImportViewModel()
            {
                FilesPendingQueueing = _importService.FindFiles(),
                FilesPendingEncode = _importService.GetQueuedFiles(),
                FilesPendingSegmenting = _importService.GetPendingSegmenting()
            };
            return View(model);
        }

        /// <summary>
        /// Upload one or more files into the /wwwroot/import/01_pending/ directory.
        /// When done, we'll replace the first pannel.
        /// </summary>
        [HttpPost]
        [RequestSizeLimit(10L * 1024L * 1024L * 1024L)]
        [RequestFormLimits(MultipartBodyLengthLimit = 10L * 1024L * 1024L * 1024L)]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var success = await _importService.UploadFile(file);

            if (success)
                return Ok();

            return StatusCode(500);
        }

        /// <summary>
        /// Scan the contents of the /wwwroot/import/01_pending/ directory. 
        /// For each encoder preset, make a copy in /wwwroot/import/02_queued/ and add a job to the database. 
        /// </summary>
        public IActionResult Queue()
        {
            _importService.IngestFiles();
            return Ok();
        }
    }
}
