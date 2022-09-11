using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenVid.Extensions;
using OpenVid.Models.Register;
using System.Linq;
using System.Threading.Tasks;

namespace OpenVid.Controllers
{
    public class RegisterController : OpenVidController
    {
        private readonly UserManager<IdentityUser> _userManager;
        public RegisterController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Index(RegisterViewModel request)
        {
            if (ModelState.IsValid)
            {
                var userCheck = await _userManager.FindByEmailAsync(request.Email);
                if (userCheck == null)
                {
                    var user = new IdentityUser
                    {
                        UserName = request.Username,
                        NormalizedUserName = request.Email,
                        Email = request.Email,
                        EmailConfirmed = true
                    };
                    var result = await _userManager.CreateAsync(user, request.Password);
                    if (result.Succeeded)
                    {
                        return RedirectToAction(SiteMap.Login);
                    }
                    else
                    {
                        if (result.Errors.Any())
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError("message", error.Description);
                            }
                        }
                        return View(request);
                    }
                }
                else
                {
                    ModelState.AddModelError("message", "Email already exists.");
                    return View(request);
                }
            }
            return View(request);

        }

    }
}
