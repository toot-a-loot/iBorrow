using iBorrow.Models;
using iBorrow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iBorrow.Controllers;

[Authorize(AuthenticationSchemes = AuthSchemes.Admin)]
public class AdminBorrowedBooksController(CirculationStore store) : Controller
{
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Data()
    {
        var (borrowed, returned) = store.GetAll();
        return Json(new { borrowed, returned });
    }

    [HttpPost]
    public IActionResult AddBorrowed([FromBody] BorrowedBook item) => Json(store.AddBorrowed(item));

    [HttpPut]
    public IActionResult EditBorrowed(string id, [FromBody] BorrowedBook item) => store.UpdateBorrowed(id, item) ? NoContent() : NotFound();

    [HttpPost]
    public IActionResult Returned(string id) => store.MarkReturned(id) is { } item ? Json(item) : NotFound();

    [HttpPost]
    public IActionResult AddReturned([FromBody] ReturnedBook item) => Json(store.AddReturned(item));
}
