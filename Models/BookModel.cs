namespace iBorrow.Models;

public class BookItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public string Synopsis { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = string.Empty;
    public int TotalCopies { get; set; } = 1;
    public string DateAdded { get; set; } = string.Empty;
}

public class BookItemDto : BookItem
{
    public bool IsAvailable { get; set; }
}
