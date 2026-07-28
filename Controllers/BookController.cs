using iBorrow.Models;
using iBorrow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iBorrow.Controllers
{
    [Authorize]
    public class BookController(BookStore books) : Controller
    {
        public IActionResult Details(string id)
        {
            var book = books.GetById(id);
            if (book is null) return NotFound();

            var similar = books.GetAll()
                .Where(b => b.Id != book.Id && b.Category == book.Category)
                .Take(6)
                .ToList();

            return View(new BookDetailsViewModel { Book = book, Similar = similar });
        }
    }
}
