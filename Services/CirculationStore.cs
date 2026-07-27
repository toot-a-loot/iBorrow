using System.Text.Json;
using iBorrow.Models;

namespace iBorrow.Services;

public sealed class CirculationStore
{
    private readonly string _filePath;
    private readonly object _sync = new();
    private List<BorrowedBook> _borrowed = [];
    private List<ReturnedBook> _returned = [];
    private List<BorrowerProfile> _borrowers = [];

    public CirculationStore(IWebHostEnvironment environment)
    {
        var directory = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(directory);
        _filePath = Path.Combine(directory, "circulation.json");
        Load();
    }

    public (IReadOnlyList<BorrowedBook> Borrowed, IReadOnlyList<ReturnedBook> Returned) GetAll()
    {
        lock (_sync) return (_borrowed.ToList(), _returned.ToList());
    }

    public IReadOnlyList<BorrowerProfile> GetBorrowers()
    {
        lock (_sync) return _borrowers.ToList();
    }

    public BorrowerProfile AddBorrower(BorrowerProfile item)
    {
        lock (_sync)
        {
            item.LibraryId = NextId(_borrowers.Select(x => x.LibraryId));
            item.Name = NormalizeName(item.Name);
            _borrowers.Add(item);
            Save();
            return item;
        }
    }

    public bool UpdateBorrower(string id, BorrowerProfile item)
    {
        lock (_sync)
        {
            var current = _borrowers.FirstOrDefault(x => x.LibraryId == id);
            if (current is null) return false;
            current.StudentId = item.StudentId; current.Name = NormalizeName(item.Name);
            current.ContactNo = item.ContactNo; current.Email = item.Email;
            Save();
            return true;
        }
    }

    public BorrowedBook AddBorrowed(BorrowedBook item)
    {
        lock (_sync)
        {
            item.Id = NextId(_borrowed.Select(x => x.Id).Concat(_returned.Select(x => x.Id)));
            item.Status = "Borrowed";
            _borrowed.Add(item);
            Save();
            return item;
        }
    }

    public bool UpdateBorrowed(string id, BorrowedBook item)
    {
        lock (_sync)
        {
            var current = _borrowed.FirstOrDefault(x => x.Id == id);
            if (current is null) return false;
            item.Id = id;
            current.Book = item.Book; current.BorrowerId = item.BorrowerId; current.BorrowerName = item.BorrowerName;
            current.DateBorrowed = item.DateBorrowed; current.DueDate = item.DueDate; current.Copies = item.Copies;
            Save();
            return true;
        }
    }

    public ReturnedBook? MarkReturned(string id)
    {
        lock (_sync)
        {
            var item = _borrowed.FirstOrDefault(x => x.Id == id);
            if (item is null) return null;
            item.Status = "Returned";
            var returned = new ReturnedBook { Id = item.Id, Book = item.Book, BorrowerId = item.BorrowerId, BorrowerName = item.BorrowerName, DateBorrowed = item.DateBorrowed, DueDate = item.DueDate, Copies = item.Copies, Status = "Returned" };
            Save();
            return returned;
        }
    }

    public ReturnedBook AddReturned(ReturnedBook item)
    {
        lock (_sync)
        {
            if (string.IsNullOrWhiteSpace(item.Id)) item.Id = NextId(_borrowed.Select(x => x.Id).Concat(_returned.Select(x => x.Id)));
            item.Status = "Returned";
            _returned.Add(item);
            Save();
            return item;
        }
    }

    private void Load()
    {
        if (!File.Exists(_filePath))
        {
            _borrowed = [new BorrowedBook { Id = "01001", Book = "Trident", BorrowerId = "c202302014", BorrowerName = "Tai Taipae", DateBorrowed = "2023-07-07", DueDate = "2026-07-10", Copies = 3 }];
            _borrowers = [new BorrowerProfile { LibraryId = "01001", StudentId = "c202401067", Name = "Doe, John", ContactNo = "901 001 0100", Email = "c202401067@iacademy.edu.ph" }];
            Save();
            return;
        }
        var data = JsonSerializer.Deserialize<StoreData>(File.ReadAllText(_filePath));
        _borrowed = data?.Borrowed ?? [];
        _returned = data?.Returned ?? [];
        _borrowers = data?.Borrowers ?? [new BorrowerProfile { LibraryId = "01001", StudentId = "c202401067", Name = "Doe, John", ContactNo = "901 001 0100", Email = "c202401067@iacademy.edu.ph" }];
    }

    private void Save() => File.WriteAllText(_filePath, JsonSerializer.Serialize(new StoreData(_borrowed, _returned, _borrowers), new JsonSerializerOptions { WriteIndented = true }));
    private static string NextId(IEnumerable<string> ids) => (ids.Select(id => int.TryParse(id, out var number) ? number : 1000).DefaultIfEmpty(1000).Max() + 1).ToString("D5");
    private static string NormalizeName(string name)
    {
        var parts = name.Split(',', 2, StringSplitOptions.TrimEntries);
        return $"{parts[0]}, {parts[1]}";
    }
    private sealed record StoreData(List<BorrowedBook> Borrowed, List<ReturnedBook> Returned, List<BorrowerProfile>? Borrowers = null);
}
