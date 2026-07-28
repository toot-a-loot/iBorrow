using iBorrow.Models;
using iBorrow.Services;
using Microsoft.AspNetCore.Mvc;

namespace iBorrow.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly AuthService _authService;

        public RegistrationController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new RegistrationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _authService.EmailExistsAsync(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
                return View(model);
            }

            await _authService.RegisterAsync(model.Email, model.Password);

            ViewBag.SuccessMessage = "Registration details submitted successfully.";
            return View(new RegistrationViewModel());
        }
    }
}
