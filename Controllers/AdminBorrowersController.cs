using iBorrow.Data;
using iBorrow.Models;
using iBorrow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace iBorrow.Controllers;

[Authorize(AuthenticationSchemes = AuthSchemes.Admin)]
public class AdminBorrowersController(CirculationStore store, UserManager<ApplicationUser> userManager) : Controller
{
    public IActionResult Index() => View();
    public IActionResult Profile() => View();

    [HttpGet]
    public IActionResult Data() => Json(store.GetBorrowers());

    [HttpGet]
    public IActionResult Overview() => Json(store.GetBorrowerOverview(DateOnly.FromDateTime(DateTime.Today)));

    [HttpPost]
    public IActionResult Add([FromBody] BorrowerProfile item) => BorrowerValidation.IsValid(item) ? Json(store.AddBorrower(item)) : BadRequest("Enter a name, student ID, and valid email.");

    [HttpPut]
    public IActionResult Edit(string id, [FromBody] BorrowerProfile item) => BorrowerValidation.IsValid(item) && store.UpdateBorrower(id, item) ? NoContent() : BadRequest("Invalid borrower details.");

    [HttpDelete]
    public async Task<IActionResult> Delete(string id)
    {
        var profile = store.GetBorrowers().FirstOrDefault(b => b.LibraryId == id);
        if (profile is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(profile.UserId))
        {
            var user = await userManager.FindByIdAsync(profile.UserId);
            if (user != null) await userManager.DeleteAsync(user);
        }

        return store.DeleteBorrower(id) ? NoContent() : NotFound();
    }
}
