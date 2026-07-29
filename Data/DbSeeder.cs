using iBorrow.Models;
using iBorrow.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace iBorrow.Data;

public static class DbSeeder
{
    public const string AdminRole = "Admin";
    public const string StudentRole = "Student";

    private static readonly List<BookItem> SeedBooks =
    [
        new()
        {
            Title = "Clean Code: A Handbook of Agile Software Craftsmanship",
            Author = "Robert C. Martin",
            Category = "Software Engineering",
            Synopsis = "A guide to writing readable, maintainable code, covering naming, functions, testing, and refactoring practices for professional developers.",
            Tags = ["Programming", "Best Practices"],
            TotalCopies = 4,
            CoverImageUrl = "/images/covers/bk-clean-code.svg"
        },
        new()
        {
            Title = "Design Patterns: Elements of Reusable Object-Oriented Software",
            Author = "Erich Gamma, Richard Helm, Ralph Johnson, John Vlissides",
            Category = "Software Engineering",
            Synopsis = "The classic catalog of 23 object-oriented design patterns that solve recurring software design problems.",
            Tags = ["Programming", "Architecture"],
            TotalCopies = 3,
            CoverImageUrl = "/images/covers/bk-design-patterns.svg"
        },
        new()
        {
            Title = "Game Programming Patterns",
            Author = "Robert Nystrom",
            Category = "Game Development",
            Synopsis = "Practical design patterns tailored to the unique problems of building games, from the game loop to component architectures.",
            Tags = ["Programming", "Game Design"],
            TotalCopies = 3,
            CoverImageUrl = "/images/covers/bk-game-programming-patterns.svg"
        },
        new()
        {
            Title = "The Art of Game Design: A Book of Lenses",
            Author = "Jesse Schell",
            Category = "Game Development",
            Synopsis = "A collection of 'lenses,' or perspectives, for evaluating and improving game designs across mechanics, story, and player experience.",
            Tags = ["Game Design", "Creativity"],
            TotalCopies = 3,
            CoverImageUrl = "/images/covers/bk-art-of-game-design.svg"
        },
        new()
        {
            Title = "The Non-Designer's Design Book",
            Author = "Robin Williams",
            Category = "Multimedia Arts",
            Synopsis = "An accessible introduction to the core principles of visual design: contrast, repetition, alignment, and proximity.",
            Tags = ["Design", "Visual Arts"],
            TotalCopies = 4,
            CoverImageUrl = "/images/covers/bk-non-designers-design-book.svg"
        },
        new()
        {
            Title = "Animator's Survival Kit",
            Author = "Richard Williams",
            Category = "Multimedia Arts",
            Synopsis = "A comprehensive manual on the principles and techniques of animation, from timing and spacing to walk cycles.",
            Tags = ["Animation", "Design"],
            TotalCopies = 2,
            CoverImageUrl = "/images/covers/bk-animators-survival-kit.svg"
        },
        new()
        {
            Title = "The Millionaire Real Estate Investor",
            Author = "Gary Keller",
            Category = "Real Estate",
            Synopsis = "Research-based strategies and models used by successful real estate investors to build wealth through property.",
            Tags = ["Investing", "Finance"],
            TotalCopies = 3,
            CoverImageUrl = "/images/covers/bk-millionaire-real-estate-investor.svg"
        },
        new()
        {
            Title = "Rich Dad Poor Dad",
            Author = "Robert T. Kiyosaki",
            Category = "Real Estate",
            Synopsis = "A personal finance classic contrasting two mindsets toward money, assets, and real estate investing.",
            Tags = ["Investing", "Finance"],
            TotalCopies = 4,
            CoverImageUrl = "/images/covers/bk-rich-dad-poor-dad.svg"
        },
        new()
        {
            Title = "Noli Me Tangere",
            Author = "Jose Rizal",
            Category = "Filipiniana",
            Synopsis = "Jose Rizal's landmark novel exposing social injustice under Spanish colonial rule in the Philippines.",
            Tags = ["Classic", "Philippine History"],
            TotalCopies = 5,
            CoverImageUrl = "/images/covers/bk-noli-me-tangere.svg"
        },
        new()
        {
            Title = "El Filibusterismo",
            Author = "Jose Rizal",
            Category = "Filipiniana",
            Synopsis = "The sequel to Noli Me Tangere, following Crisostomo Ibarra's return as he plots revolution against colonial rule.",
            Tags = ["Classic", "Philippine History"],
            TotalCopies = 4,
            CoverImageUrl = "/images/covers/bk-el-filibusterismo.svg"
        },
        new()
        {
            Title = "Sapiens: A Brief History of Humankind",
            Author = "Yuval Noah Harari",
            Category = "Others",
            Synopsis = "A sweeping account of how Homo sapiens came to dominate the planet, from the Cognitive Revolution to the present.",
            Tags = ["History", "Non-Fiction"],
            TotalCopies = 4,
            CoverImageUrl = "/images/covers/bk-sapiens.svg"
        },
        new()
        {
            Title = "Atomic Habits",
            Author = "James Clear",
            Category = "Others",
            Synopsis = "A practical framework for building good habits and breaking bad ones through small, consistent changes.",
            Tags = ["Self-Help", "Non-Fiction"],
            TotalCopies = 5,
            CoverImageUrl = "/images/covers/bk-atomic-habits.svg"
        }
    ];

    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var db = provider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { AdminRole, StudentRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        if (!db.Books.Any())
        {
            var bookStore = provider.GetRequiredService<BookStore>();
            foreach (var book in SeedBooks)
                bookStore.Add(new BookItem
                {
                    Title = book.Title,
                    Author = book.Author,
                    Category = book.Category,
                    Synopsis = book.Synopsis,
                    Tags = book.Tags,
                    TotalCopies = book.TotalCopies,
                    CoverImageUrl = book.CoverImageUrl
                });
        }

        var adminEmail = configuration["Seed:AdminEmail"];
        var adminPassword = configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            return;

        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var existingAdmins = await userManager.GetUsersInRoleAsync(AdminRole);
        var admin = existingAdmins.FirstOrDefault();

        foreach (var extra in existingAdmins.Skip(1))
            await userManager.DeleteAsync(extra);

        if (admin is null)
        {
            admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                var result = await userManager.CreateAsync(admin, adminPassword);
                if (!result.Succeeded)
                    throw new InvalidOperationException($"Failed to seed admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            await userManager.AddToRoleAsync(admin, AdminRole);
            return;
        }

        if (!string.Equals(admin.Email, adminEmail, StringComparison.OrdinalIgnoreCase))
        {
            admin.UserName = adminEmail;
            admin.Email = adminEmail;
            admin.EmailConfirmed = true;
            await userManager.UpdateAsync(admin);
        }

        if (!await userManager.CheckPasswordAsync(admin, adminPassword))
        {
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(admin);
            await userManager.ResetPasswordAsync(admin, resetToken, adminPassword);
        }
    }
}
