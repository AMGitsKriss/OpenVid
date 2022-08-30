using Microsoft.AspNetCore.Mvc;

namespace OpenVid.Areas.Tags.Controllers
{
    [Area("Tags")]
    public class ManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
