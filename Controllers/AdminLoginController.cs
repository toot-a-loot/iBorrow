using System.Security.Claims;
using iBorrow.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iBorrow.Controllers
{
    public class AdminLoginController : Controller
    {
        private readonly AuthService _authService;

        public AdminLoginController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string email, string password)
        {
            var user = await _authService.ValidateCredentialsAsync(email, password);
            if (user is null || !user.IsAdmin)
            {
                ViewBag.ErrorMessage = "Invalid email or password.";
                return View();
            }

            var claims = new List<Claim> { new(ClaimTypes.Name, user.Email) };
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
