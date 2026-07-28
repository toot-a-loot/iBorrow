using System.Security.Claims;
using iBorrow.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace iBorrow.Controllers
{
    public class AdminLoginController(UserManager<ApplicationUser> userManager) : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string email, string password)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null || !await userManager.CheckPasswordAsync(user, password) ||
                !await userManager.IsInRoleAsync(user, DbSeeder.AdminRole))
            {
                ViewBag.ErrorMessage = "Invalid email or password.";
                return View();
            }

            var claims = new List<Claim> { new(ClaimTypes.Name, email), new(ClaimTypes.NameIdentifier, user.Id) };
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
