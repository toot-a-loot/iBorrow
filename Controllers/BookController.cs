using System.Security.Claims;
using iBorrow.Data;
using iBorrow.Models;
using iBorrow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iBorrow.Controllers
{
    [Authorize]
    public class BookController(BookStore books, CirculationStore circulation, AppDbContext db) : Controller
    {
        public IActionResult Details(string id)
        {
            var book = books.GetById(id);
            if (book is null) return NotFound();

            var similar = books.GetAll()
                .Where(b => b.Id != book.Id && b.Category == book.Category)
                .Take(6)
                .ToList();

            var borrowDate = DateTime.Today;
            return View(new BookDetailsViewModel
            {
                Book = book,
                Similar = similar,
                BorrowDate = borrowDate,
                DueDate = NextMonday(borrowDate)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Borrow(string id)
        {
            var book = books.GetById(id);
            if (book is null) return NotFound();
            if (!book.IsAvailable) return BadRequest("This book is no longer available.");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var profile = db.Borrowers.FirstOrDefault(b => b.UserId == userId);
            if (profile is null) return BadRequest("Borrower profile not found.");

            var borrowDate = DateTime.Today;
            var dueDate = NextMonday(borrowDate);

            circulation.AddBorrowed(new BorrowedBook
            {
                Book = book.Id,
                BorrowerId = profile.StudentId,
                BorrowerName = profile.Name,
                DateBorrowed = borrowDate.ToString("yyyy-MM-dd"),
                DueDate = dueDate.ToString("yyyy-MM-dd"),
                Copies = 1
            });

            return Ok();
        }

        private static DateTime NextMonday(DateTime from)
        {
            var daysUntilMonday = ((int)DayOfWeek.Monday - (int)from.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0) daysUntilMonday = 7;
            return from.AddDays(daysUntilMonday);
        }
    }
}
