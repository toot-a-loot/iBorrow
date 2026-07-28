using iBorrow.Data;
using iBorrow.Models;
using Microsoft.EntityFrameworkCore;

namespace iBorrow.Services;

public sealed class CirculationStore(AppDbContext db)
{
    public (IReadOnlyList<BorrowedBook> Borrowed, IReadOnlyList<ReturnedBook> Returned) GetAll()
    {
        var loans = db.Loans.AsNoTracking().ToList();
        var borrowed = loans.Where(l => !string.Equals(l.Status, "Returned", StringComparison.OrdinalIgnoreCase))
            .Select(ToBorrowedBook).ToList();
        var returned = loans.Where(l => string.Equals(l.Status, "Returned", StringComparison.OrdinalIgnoreCase))
            .Select(ToReturnedBook).ToList();
        return (borrowed, returned);
    }

    public IReadOnlyList<BorrowerProfile> GetBorrowers() => [.. db.Borrowers.AsNoTracking()];

    public IReadOnlyList<BorrowerOverview> GetBorrowerOverview(DateOnly today)
    {
        var borrowers = db.Borrowers.AsNoTracking().ToList();
        var loans = db.Loans.AsNoTracking()
            .Where(l => l.Status != "Returned")
            .ToList();

        return loans
            .GroupBy(item => item.BorrowerId)
            .Select(group =>
            {
                var profile = borrowers.FirstOrDefault(item => item.StudentId == group.Key || item.LibraryId == group.Key);
                var loanList = group.ToList();
                var dueDates = loanList.Select(DueDateFor).ToList();
                var dueDate = dueDates.Min();
                var borrowedDate = loanList.Select(item => ParseDate(item.DateBorrowed)).Min();
                var daysRemaining = dueDate.DayNumber - today.DayNumber;
                var status = daysRemaining < 0 ? "Overdue" : daysRemaining == 1 ? "Nearly Due" : "Normal";
                return new BorrowerOverview
                {
                    LibraryId = profile?.LibraryId ?? loanList[0].Id,
                    Name = profile?.Name ?? loanList[0].BorrowerName,
                    StudentId = profile?.StudentId ?? loanList[0].BorrowerId,
                    BorrowedBooks = loanList.Sum(item => Math.Max(item.Copies, 1)),
                    BookTitles = loanList.Select(item => item.Book).Where(item => !string.IsNullOrWhiteSpace(item)).Distinct().ToList(),
                    Status = status,
                    DueDate = dueDate.ToString("yyyy-MM-dd"),
                    DateBorrowed = borrowedDate.ToString("yyyy-MM-dd"),
                    DaysRemaining = daysRemaining
                };
            })
            .OrderBy(item => item.Status == "Normal" ? 1 : 0)
            .ThenBy(item => item.Status == "Overdue" ? item.DaysRemaining : int.MaxValue)
            .ThenBy(item => item.DaysRemaining)
            .ToList();
    }

    public BorrowerDetail? GetBorrowerDetail(string studentIdOrLibraryId)
    {
        var profile = db.Borrowers.AsNoTracking()
            .FirstOrDefault(b => b.StudentId == studentIdOrLibraryId || b.LibraryId == studentIdOrLibraryId);
        if (profile is null) return null;

        var loans = db.Loans.AsNoTracking()
            .Where(l => l.BorrowerId == profile.StudentId || l.BorrowerId == profile.LibraryId)
            .OrderByDescending(l => l.DateBorrowed)
            .ToList();

        return new BorrowerDetail
        {
            LibraryId = profile.LibraryId,
            StudentId = profile.StudentId,
            Name = profile.Name,
            Email = profile.Email,
            Loans = loans.Select(l => new BorrowerLoanRecord
            {
                Book = l.Book,
                DateBorrowed = l.DateBorrowed,
                DueDate = l.DueDate,
                Status = l.Status,
                DateReturned = l.DateReturned ?? string.Empty
            }).ToList()
        };
    }

    private static DateOnly DueDateFor(Loan item) =>
        TryParseDate(item.DueDate, out var dueDate) ? dueDate : ParseDate(item.DateBorrowed).AddDays(7);

    private static DateOnly ParseDate(string value) => TryParseDate(value, out var date) ? date : DateOnly.FromDateTime(DateTime.Today);
    private static bool TryParseDate(string value, out DateOnly date) => DateOnly.TryParse(value, out date);

    public BorrowerProfile AddBorrower(BorrowerProfile item)
    {
        item.LibraryId = NextId(db.Borrowers.Select(x => x.LibraryId));
        item.Name = NormalizeName(item.Name);
        db.Borrowers.Add(item);
        db.SaveChanges();
        return item;
    }

    public bool UpdateBorrower(string id, BorrowerProfile item)
    {
        var current = db.Borrowers.FirstOrDefault(x => x.LibraryId == id);
        if (current is null) return false;
        current.StudentId = item.StudentId; current.Name = NormalizeName(item.Name); current.Email = item.Email;
        db.SaveChanges();
        return true;
    }

    public bool DeleteBorrower(string id)
    {
        var current = db.Borrowers.FirstOrDefault(x => x.LibraryId == id);
        if (current is null) return false;
        db.Borrowers.Remove(current);
        db.SaveChanges();
        return true;
    }

    public BorrowedBook AddBorrowed(BorrowedBook item)
    {
        var loan = new Loan
        {
            Id = NextId(db.Loans.Select(x => x.Id)),
            Book = item.Book,
            BorrowerId = item.BorrowerId,
            BorrowerName = item.BorrowerName,
            DateBorrowed = item.DateBorrowed,
            DueDate = item.DueDate,
            Copies = item.Copies,
            Status = "Borrowed"
        };
        db.Loans.Add(loan);
        db.SaveChanges();
        return ToBorrowedBook(loan);
    }

    public bool UpdateBorrowed(string id, BorrowedBook item)
    {
        var current = db.Loans.FirstOrDefault(x => x.Id == id);
        if (current is null) return false;
        current.Book = item.Book; current.BorrowerId = item.BorrowerId; current.BorrowerName = item.BorrowerName;
        current.DateBorrowed = item.DateBorrowed; current.DueDate = item.DueDate; current.Copies = item.Copies;
        db.SaveChanges();
        return true;
    }

    public ReturnedBook? MarkReturned(string id)
    {
        var loan = db.Loans.FirstOrDefault(x => x.Id == id);
        if (loan is null) return null;
        loan.Status = "Returned";
        db.SaveChanges();
        return ToReturnedBook(loan);
    }

    public ReturnedBook AddReturned(ReturnedBook item)
    {
        var loan = new Loan
        {
            Id = string.IsNullOrWhiteSpace(item.Id) ? NextId(db.Loans.Select(x => x.Id)) : item.Id,
            Book = item.Book,
            BorrowerId = item.BorrowerId,
            BorrowerName = item.BorrowerName,
            DateBorrowed = item.DateBorrowed,
            DueDate = item.DueDate,
            Copies = item.Copies,
            Status = "Returned",
            ProcessedBy = item.ProcessedBy,
            DateReturned = item.DateReturned,
            ReceivedBy = item.ReceivedBy
        };
        db.Loans.Add(loan);
        db.SaveChanges();
        return ToReturnedBook(loan);
    }

    private static BorrowedBook ToBorrowedBook(Loan loan) => new()
    {
        Id = loan.Id, Book = loan.Book, BorrowerId = loan.BorrowerId, BorrowerName = loan.BorrowerName,
        DateBorrowed = loan.DateBorrowed, DueDate = loan.DueDate, Copies = loan.Copies, Status = loan.Status
    };

    private static ReturnedBook ToReturnedBook(Loan loan) => new()
    {
        Id = loan.Id, Book = loan.Book, BorrowerId = loan.BorrowerId, BorrowerName = loan.BorrowerName,
        DateBorrowed = loan.DateBorrowed, DueDate = loan.DueDate, Copies = loan.Copies, Status = loan.Status,
        ProcessedBy = loan.ProcessedBy ?? string.Empty, DateReturned = loan.DateReturned ?? string.Empty, ReceivedBy = loan.ReceivedBy ?? string.Empty
    };

    private static string NextId(IEnumerable<string> ids) => (ids.Select(id => int.TryParse(id, out var number) ? number : 1000).DefaultIfEmpty(1000).Max() + 1).ToString("D5");

    private static string NormalizeName(string name)
    {
        var parts = name.Split(',', 2, StringSplitOptions.TrimEntries);
        return parts.Length == 2 ? $"{parts[0]}, {parts[1]}" : parts[0];
    }
}
