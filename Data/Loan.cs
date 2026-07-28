namespace iBorrow.Data;

public class Loan
{
    public string Id { get; set; } = string.Empty;
    public string Book { get; set; } = string.Empty;
    public string BorrowerId { get; set; } = string.Empty;
    public string BorrowerName { get; set; } = string.Empty;
    public string DateBorrowed { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
    public int Copies { get; set; }
    public string Status { get; set; } = "Borrowed";
    public string? ProcessedBy { get; set; }
    public string? DateReturned { get; set; }
    public string? ReceivedBy { get; set; }
}
