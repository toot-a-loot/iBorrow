using iBorrow.Models;
using iBorrow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iBorrow.Controllers;

[Authorize(AuthenticationSchemes = AuthSchemes.Admin)]
public class AdminBorrowersController(CirculationStore store) : Controller
{
    public IActionResult Index() => View();
    public IActionResult Profile() => View();

    [HttpGet]
    public IActionResult Data() => Json(store.GetBorrowers());

    [HttpGet]
    public IActionResult Overview() => Json(store.GetBorrowerOverview(DateOnly.FromDateTime(DateTime.Today)));

    [HttpPost]
    public IActionResult Add([FromBody] BorrowerProfile item) => BorrowerValidation.IsValid(item) ? Json(store.AddBorrower(item)) : BadRequest("Name must use the format Lastname, Firstname.");

    [HttpPut]
    public IActionResult Edit(string id, [FromBody] BorrowerProfile item) => BorrowerValidation.IsValid(item) && store.UpdateBorrower(id, item) ? NoContent() : BadRequest("Invalid borrower details.");
}
