namespace iBorrow.Models;

public class BorrowedBook
{
    public string Id { get; set; } = string.Empty;
    public string Book { get; set; } = string.Empty;
    public string BorrowerId { get; set; } = string.Empty;
    public string BorrowerName { get; set; } = string.Empty;
    public string DateBorrowed { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
    public int Copies { get; set; }
    public string Status { get; set; } = "Borrowed";
}

public class ReturnedBook : BorrowedBook
{
    public string ProcessedBy { get; set; } = string.Empty;
    public string DateReturned { get; set; } = string.Empty;
    public string ReceivedBy { get; set; } = string.Empty;
}

public class BorrowerProfile
{
    public string LibraryId { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;
    public string ContactNo { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? UserId { get; set; }
}

public class BorrowerOverview
{
    public string LibraryId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;
    public int BorrowedBooks { get; set; }
    public List<string> BookTitles { get; set; } = [];
    public string Status { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
    public string DateBorrowed { get; set; } = string.Empty;
    public int DaysRemaining { get; set; }
}
