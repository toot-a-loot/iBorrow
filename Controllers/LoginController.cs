using System.Security.Claims;
using iBorrow.Data;
using iBorrow.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace iBorrow.Controllers
{
    public class LoginController(UserManager<ApplicationUser> userManager, AppDbContext db) : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user is null || !await userManager.CheckPasswordAsync(user, model.Password) ||
                !await userManager.IsInRoleAsync(user, DbSeeder.StudentRole))
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            var displayName = db.Borrowers.FirstOrDefault(b => b.UserId == user.Id)?.Name ?? model.Email.Split('@')[0];
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, model.Email),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(AccountController.DisplayNameClaimType, displayName),
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }
    }
}
