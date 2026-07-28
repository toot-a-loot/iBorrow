using iBorrow.Data;
using iBorrow.Models;
using Microsoft.EntityFrameworkCore;

namespace iBorrow.Services;

public sealed class BookStore(AppDbContext db, CirculationStore circulation)
{
    private static readonly List<string> DefaultTags =
        ["Action", "Adventure", "Comedy", "Drama", "Fantasy", "Horror", "Romance"];

    private static readonly List<string> Categories =
        ["Software Engineering", "Game Development", "Multimedia Arts", "Real Estate", "Filipiniana"];

    // ── Queries ──────────────────────────────────────────────────────────────

    public IReadOnlyList<string> GetCategories() => Categories.AsReadOnly();

    public IReadOnlyList<string> GetTags()
    {
        EnsureDefaultTags();
        return [.. db.Tags.Select(t => t.Name).OrderBy(name => name)];
    }

    /// <summary>Returns all books enriched with live availability.</summary>
    public IReadOnlyList<BookItemDto> GetAll()
    {
        var books = db.Books.AsNoTracking().ToList();
        var activeBorrows = circulation.GetAll().Borrowed
            .Where(b => !string.Equals(b.Status, "Returned", StringComparison.OrdinalIgnoreCase))
            .ToList();
        return books.Select(b => Enrich(b, activeBorrows)).ToList();
    }

    /// <summary>Real-time search: matches title, author, category, tags.</summary>
    public IReadOnlyList<BookItemDto> Search(string query, IEnumerable<string>? categories, IEnumerable<string>? tags)
    {
        var all = GetAll();
        var q = (query ?? string.Empty).Trim().ToLowerInvariant();
        var cats = categories?.Where(c => !string.IsNullOrWhiteSpace(c)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var tagSet = tags?.Where(t => !string.IsNullOrWhiteSpace(t)).ToHashSet(StringComparer.OrdinalIgnoreCase);

        return all.Where(b =>
        {
            if (!string.IsNullOrEmpty(q))
            {
                var haystack = $"{b.Title} {b.Author} {b.Category} {string.Join(' ', b.Tags)}".ToLowerInvariant();
                if (!haystack.Contains(q)) return false;
            }
            if (cats is { Count: > 0 } && !cats.Contains(b.Category)) return false;
            if (tagSet is { Count: > 0 } && !b.Tags.Any(t => tagSet.Contains(t))) return false;
            return true;
        }).ToList();
    }

    // ── Mutations ─────────────────────────────────────────────────────────────

    public BookItem Add(BookItem item)
    {
        item.Id = NextId();
        item.DateAdded = DateTime.Today.ToString("yyyy-MM-dd");
        if (item.TotalCopies < 1) item.TotalCopies = 1;
        db.Books.Add(item);
        foreach (var tag in item.Tags.Where(t => !string.IsNullOrWhiteSpace(t)))
            EnsureTag(tag);
        db.SaveChanges();
        return item;
    }

    public bool AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) return false;
        if (!EnsureTag(tag.Trim())) return false;
        db.SaveChanges();
        return true;
    }

    public bool Update(BookItem item)
    {
        if (item == null || string.IsNullOrWhiteSpace(item.Id)) return false;

        var existing = db.Books.FirstOrDefault(b => b.Id == item.Id);
        if (existing == null) return false;

        existing.Title = item.Title;
        existing.Author = item.Author;
        existing.Category = item.Category;
        existing.Synopsis = item.Synopsis;
        existing.Tags = item.Tags;
        existing.TotalCopies = Math.Max(item.TotalCopies, 1);
        if (!string.IsNullOrWhiteSpace(item.CoverImageUrl))
        {
            existing.CoverImageUrl = item.CoverImageUrl;
        }

        foreach (var tag in item.Tags.Where(t => !string.IsNullOrWhiteSpace(t)))
            EnsureTag(tag);

        db.SaveChanges();
        return true;
    }

    public bool Delete(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return false;

        var existing = db.Books.FirstOrDefault(b => b.Id == id);
        if (existing == null) return false;

        db.Books.Remove(existing);
        db.SaveChanges();
        return true;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static BookItemDto Enrich(BookItem b, List<BorrowedBook> activeBorrows)
    {
        var borrowed = activeBorrows
            .Where(r => string.Equals(r.Book, b.Title, StringComparison.OrdinalIgnoreCase)
                     || string.Equals(r.Book, b.Id, StringComparison.OrdinalIgnoreCase))
            .Sum(r => Math.Max(r.Copies, 1));

        return new BookItemDto
        {
            Id = b.Id, Title = b.Title, Author = b.Author, Category = b.Category,
            Tags = b.Tags, Synopsis = b.Synopsis, CoverImageUrl = b.CoverImageUrl,
            TotalCopies = b.TotalCopies, DateAdded = b.DateAdded,
            IsAvailable = borrowed < b.TotalCopies
        };
    }

    /// <returns>true if the tag was newly added; false if it already existed.</returns>
    private bool EnsureTag(string tag)
    {
        EnsureDefaultTags();
        if (db.Tags.Local.Any(t => string.Equals(t.Name, tag, StringComparison.OrdinalIgnoreCase)) ||
            db.Tags.Any(t => t.Name.ToLower() == tag.ToLower()))
            return false;
        db.Tags.Add(new LibraryTag { Name = tag });
        return true;
    }

    private void EnsureDefaultTags()
    {
        if (db.Tags.Any()) return;
        db.Tags.AddRange(DefaultTags.Select(name => new LibraryTag { Name = name }));
        db.SaveChanges();
    }

    private string NextId()
    {
        var max = db.Books.Select(b => b.Id)
            .AsEnumerable()
            .Select(id => int.TryParse(id?.Replace("BK", ""), out var n) ? n : 0)
            .DefaultIfEmpty(0).Max();
        return $"BK{(max + 1):D5}";
    }
}
