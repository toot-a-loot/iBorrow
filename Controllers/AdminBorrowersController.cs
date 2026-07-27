using iBorrow.Models;
using iBorrow.Services;
using Microsoft.AspNetCore.Mvc;

namespace iBorrow.Controllers;

public class AdminBorrowersController(CirculationStore store) : Controller
{
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Data() => Json(store.GetBorrowers());

    [HttpPost]
    public IActionResult Add([FromBody] BorrowerProfile item) => IsValid(item) ? Json(store.AddBorrower(item)) : BadRequest("Name must use the format Lastname, Firstname.");

    [HttpPut]
    public IActionResult Edit(string id, [FromBody] BorrowerProfile item) => IsValid(item) && store.UpdateBorrower(id, item) ? NoContent() : BadRequest("Invalid borrower details.");

    private static bool IsValid(BorrowerProfile item) =>
        !string.IsNullOrWhiteSpace(item.StudentId) && !string.IsNullOrWhiteSpace(item.Name) &&
        item.Name.Count(c => c == ',') == 1 && item.Name.Split(',', 2).All(part => !string.IsNullOrWhiteSpace(part)) &&
        !string.IsNullOrWhiteSpace(item.ContactNo) && !string.IsNullOrWhiteSpace(item.Email) &&
        IsEmail(item.Email);

    private static bool IsEmail(string email)
    {
        try { return new System.Net.Mail.MailAddress(email).Address == email; }
        catch (FormatException) { return false; }
    }
}
