using iBorrow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iBorrow.Controllers
{
    [Authorize]
    public class LibraryController(BookStore books) : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Categories = books.GetCategories();
            return View();
        }

        [HttpGet]
        public IActionResult Data() => Json(books.GetAll());
    }
}
