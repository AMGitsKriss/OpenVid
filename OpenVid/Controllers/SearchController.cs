using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenVid.Models;
using OpenVid.Models.Search;

namespace OpenVid.Controllers
{
    public class SearchController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public SearchController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Route("[Controller]/{searchString}")]
        public IActionResult Index(string searchString)
        {
            Videos video = new Videos();

            SearchViewModel viewModel = new SearchViewModel()
            {
                Videos = video.Search(searchString),
                Tags = video.GetAllTags(),
                SearchString = searchString
            };

            return View(viewModel);
        }
    }
}
