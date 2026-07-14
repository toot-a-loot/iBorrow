using iBorrow.Models;
using Microsoft.AspNetCore.Mvc;

namespace iBorrow.Controllers
{
    public class RegistrationController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View(new RegistrationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(RegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ViewBag.SuccessMessage = "Registration details submitted successfully.";
            return View(new RegistrationViewModel());
        }
    }
}