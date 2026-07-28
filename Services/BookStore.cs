using System.Text.Json;
using iBorrow.Models;

namespace iBorrow.Services;

public sealed class BookStore
{
    private static readonly List<string> DefaultTags =
        ["Action", "Adventure", "Comedy", "Drama", "Fantasy", "Horror", "Romance"];

    private static readonly List<string> Categories =
        ["Software Engineering", "Game Development", "Multimedia Arts", "Real Estate", "Filipiniana"];

    private readonly string _filePath;
    private readonly CirculationStore _circulation;
    private readonly object _sync = new();
    private List<BookItem> _books = [];
    private List<string> _tags = [];

    public BookStore(IWebHostEnvironment environment, CirculationStore circulation)
    {
        var directory = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(directory);
        _filePath = Path.Combine(directory, "books.json");
        _circulation = circulation;
        Load();
    }

    // ── Queries ──────────────────────────────────────────────────────────────

    public IReadOnlyList<string> GetCategories() => Categories.AsReadOnly();

    public IReadOnlyList<string> GetTags()
    {
        lock (_sync) return _tags.ToList();
    }

    /// <summary>Returns all books enriched with live availability.</summary>
    public IReadOnlyList<BookItemDto> GetAll()
    {
        List<BookItem> books;
        lock (_sync) books = _books.ToList();
        var activeBorrows = _circulation.GetAll().Borrowed
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
        lock (_sync)
        {
            item.Id = NextId();
            item.DateAdded = DateTime.Today.ToString("yyyy-MM-dd");
            if (item.TotalCopies < 1) item.TotalCopies = 1;
            _books.Add(item);
            // Ensure any new tags are persisted
            foreach (var tag in item.Tags.Where(t => !string.IsNullOrWhiteSpace(t)))
                EnsureTag(tag);
            Save();
            return item;
        }
    }

    public bool AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) return false;
        lock (_sync)
        {
            if (EnsureTag(tag.Trim()))
            {
                Save();
                return true;
            }
            return false;
        }
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
        if (_tags.Any(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase))) return false;
        _tags.Add(tag);
        return true;
    }

    private string NextId()
    {
        var max = _books.Select(b => int.TryParse(b.Id?.Replace("BK", ""), out var n) ? n : 0).DefaultIfEmpty(0).Max();
        return $"BK{(max + 1):D5}";
    }

    private void Load()
    {
        if (!File.Exists(_filePath))
        {
            _books = [];
            _tags = [.. DefaultTags];
            Save();
            return;
        }
        var data = JsonSerializer.Deserialize<StoreData>(File.ReadAllText(_filePath));
        _books = data?.Books ?? [];
        _tags = data?.Tags is { Count: > 0 } ? data.Tags : [.. DefaultTags];
    }

    private void Save() =>
        File.WriteAllText(_filePath, JsonSerializer.Serialize(
            new StoreData(_books, _tags),
            new JsonSerializerOptions { WriteIndented = true }));

    private sealed record StoreData(List<BookItem> Books, List<string> Tags);
}
