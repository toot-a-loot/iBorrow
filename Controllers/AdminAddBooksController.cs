using iBorrow.Models;
using iBorrow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iBorrow.Controllers;

[Authorize(AuthenticationSchemes = AuthSchemes.Admin)]
public class AdminAddBooksController(BookStore books, IWebHostEnvironment env) : Controller
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    // GET /AdminAddBooks
    public IActionResult Index() => View();

    // GET /AdminAddBooks/Books
    [HttpGet]
    public IActionResult Books() => Json(books.GetAll());

    // GET /AdminAddBooks/Tags
    [HttpGet]
    public IActionResult Tags() => Json(books.GetTags());

    // GET /AdminAddBooks/Categories
    [HttpGet]
    public IActionResult Categories() => Json(books.GetCategories());

    // POST /AdminAddBooks/AddTag
    [HttpPost]
    public IActionResult AddTag([FromBody] TagRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Tag))
            return BadRequest("Tag name is required.");
        books.AddTag(request.Tag.Trim());
        return Json(new { tag = request.Tag.Trim() });
    }

    // POST /AdminAddBooks/Add  (multipart/form-data)
    [HttpPost]
    public async Task<IActionResult> Add(
        [FromForm] string title,
        [FromForm] string author,
        [FromForm] string category,
        [FromForm] string synopsis,
        [FromForm] string? tags,
        [FromForm] int totalCopies,
        IFormFile? coverImage)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author) ||
            string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(synopsis))
            return BadRequest("Title, Author, Category and Synopsis are required.");

        var coverUrl = string.Empty;

        if (coverImage is { Length: > 0 })
        {
            var ext = Path.GetExtension(coverImage.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return BadRequest("Only PNG, JPG, JPEG and WEBP images are accepted.");
            if (coverImage.Length > MaxFileSizeBytes)
                return BadRequest("Image must be smaller than 5 MB.");

            var uploadDir = Path.Combine(env.WebRootPath, "uploads", "books");
            Directory.CreateDirectory(uploadDir);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadDir, fileName);
            await using var stream = System.IO.File.Create(fullPath);
            await coverImage.CopyToAsync(stream);
            coverUrl = $"/uploads/books/{fileName}";
        }

        var tagList = (tags ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var item = new BookItem
        {
            Title = title.Trim(),
            Author = author.Trim(),
            Category = category.Trim(),
            Synopsis = synopsis.Trim(),
            Tags = tagList,
            CoverImageUrl = coverUrl,
            TotalCopies = Math.Max(totalCopies, 1)
        };

        var saved = books.Add(item);
        return Json(saved);
    }

    public sealed class TagRequest
    {
        public string? Tag { get; set; }
    }
}
