using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iBorrow.Controllers
{
    public class AdminLoginController : Controller
    {
        private const string HardcodedEmail = "user@test.com";
        private const string HardcodedPassword = "password";

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string email, string password)
        {
            if (email != HardcodedEmail || password != HardcodedPassword)
            {
                ViewBag.ErrorMessage = "Invalid email or password.";
                return View();
            }

            var claims = new List<Claim> { new(ClaimTypes.Name, email) };
            var identity = new ClaimsIdentity(claims, AuthSchemes.Admin);
            await HttpContext.SignInAsync(AuthSchemes.Admin, new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "AdminBorrowers");
        }

        [Authorize(AuthenticationSchemes = AuthSchemes.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(AuthSchemes.Admin);
            return RedirectToAction("Index", "AdminLogin");
        }
    }
}
