using System.Security.Claims;
using iBorrow.Data;
using iBorrow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iBorrow.Controllers
{
    [Authorize]
    public class BorrowingController(CirculationStore circulation, BookStore books, AppDbContext db) : Controller
    {
        public IActionResult Index() => View();

        [HttpGet]
        public IActionResult Data()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var profile = db.Borrowers.FirstOrDefault(b => b.UserId == userId);
            var loans = profile != null ? circulation.GetBorrowerDetail(profile.StudentId)?.Loans ?? [] : [];
            var today = DateOnly.FromDateTime(DateTime.Today);

            var enriched = loans.Select(loan =>
            {
                var book = books.GetById(loan.Book);
                DateOnly.TryParse(loan.DueDate, out var dueDate);
                DateOnly.TryParse(loan.DateReturned, out var returnedOn);
                return new
                {
                    bookId = loan.Book,
                    title = book?.Title ?? loan.Book,
                    author = book?.Author ?? string.Empty,
                    coverImageUrl = book?.CoverImageUrl ?? string.Empty,
                    dateBorrowed = loan.DateBorrowed,
                    dueDate = loan.DueDate,
                    dateReturned = loan.DateReturned,
                    daysRemaining = dueDate.DayNumber - today.DayNumber,
                    isReturned = string.Equals(loan.Status, "Returned", StringComparison.OrdinalIgnoreCase),
                    returnedOn
                };
            }).ToList();

            var overdue = enriched.Where(e => !e.isReturned && e.daysRemaining < 0).OrderBy(e => e.daysRemaining).ToList();
            var dueThisWeek = enriched.Where(e => !e.isReturned && e.daysRemaining >= 0).OrderBy(e => e.daysRemaining).ToList();
            var returnedCutoff = today.AddMonths(-3);
            var returned = enriched.Where(e => e.isReturned && e.returnedOn >= returnedCutoff).OrderByDescending(e => e.returnedOn).ToList();

            return Json(new
            {
                overdueCount = overdue.Count,
                dueThisWeekCount = dueThisWeek.Count,
                activeCount = overdue.Count + dueThisWeek.Count,
                returnedCount = enriched.Count(e => e.isReturned),
                overdue = overdue.Select(e => new { e.bookId, e.title, e.author, e.coverImageUrl, e.dateBorrowed, e.dueDate, e.daysRemaining }),
                dueThisWeek = dueThisWeek.Select(e => new { e.bookId, e.title, e.author, e.coverImageUrl, e.dateBorrowed, e.dueDate, e.daysRemaining }),
                returned = returned.Select(e => new { e.bookId, e.title, e.author, e.coverImageUrl, e.dateBorrowed, e.dateReturned })
            });
        }
    }
}
