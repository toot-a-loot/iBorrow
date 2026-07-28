using iBorrow.Data;
using iBorrow.Models;
using iBorrow.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace iBorrow.Controllers
{
    public class RegistrationController(UserManager<ApplicationUser> userManager, AppDbContext db, CirculationStore circulation) : Controller
    {
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

            if (await userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
                return View(model);
            }

            if (db.Borrowers.Any(b => b.StudentId == model.StudentId))
            {
                ModelState.AddModelError(nameof(model.StudentId), "An account with this Student ID already exists.");
                return View(model);
            }

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            await userManager.AddToRoleAsync(user, DbSeeder.StudentRole);

            circulation.AddBorrower(new BorrowerProfile
            {
                StudentId = model.StudentId,
                Name = model.Name,
                Email = model.Email,
                UserId = user.Id
            });

            TempData["SuccessMessage"] = "Registration successful. Please log in.";
            return RedirectToAction("Index", "Login");
        }
    }
}
