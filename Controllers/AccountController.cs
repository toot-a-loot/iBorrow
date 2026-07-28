using System.Security.Claims;
using iBorrow.Data;
using iBorrow.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace iBorrow.Controllers
{
    [Authorize]
    public class AccountController(AppDbContext db, UserManager<ApplicationUser> userManager) : Controller
    {
        public const string DisplayNameClaimType = "DisplayName";

        [HttpGet]
        public IActionResult Index()
        {
            return View(BuildViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rename(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                ModelState.AddModelError(string.Empty, "Name cannot be empty.");
                return View("Index", BuildViewModel());
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var profile = db.Borrowers.FirstOrDefault(b => b.UserId == userId);
            if (profile != null)
            {
                profile.Name = displayName.Trim();
                db.SaveChanges();
            }

            var identity = (ClaimsIdentity)User.Identity!;
            var existing = identity.FindFirst(DisplayNameClaimType);
            if (existing != null)
            {
                identity.RemoveClaim(existing);
            }
            identity.AddClaim(new Claim(DisplayNameClaimType, displayName.Trim()));

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            TempData["SuccessMessage"] = "Name updated.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var profile = db.Borrowers.FirstOrDefault(b => b.UserId == userId);
            if (profile != null)
            {
                db.Borrowers.Remove(profile);
                db.SaveChanges();
            }

            var user = await userManager.FindByIdAsync(userId!);
            if (user != null)
            {
                await userManager.DeleteAsync(user);
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }

        private AccountViewModel BuildViewModel() => new()
        {
            DisplayName = User.FindFirst(DisplayNameClaimType)?.Value ?? string.Empty,
            Email = User.Identity?.Name ?? string.Empty,
        };
    }
}
