using System.Security.Claims;
using iBorrow.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iBorrow.Controllers
{
    [Authorize]
    public class AccountController : Controller
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
