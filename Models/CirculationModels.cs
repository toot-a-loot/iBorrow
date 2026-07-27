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
