using iBorrow.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace iBorrow.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<BookItem> Books => Set<BookItem>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<BorrowerProfile> Borrowers => Set<BorrowerProfile>();
    public DbSet<LibraryTag> Tags => Set<LibraryTag>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Ignore<BookItemDto>();

        builder.Entity<BookItem>(entity =>
        {
            entity.HasKey(b => b.Id);
        });

        builder.Entity<Loan>(entity =>
        {
            entity.HasKey(l => l.Id);
        });

        builder.Entity<BorrowerProfile>(entity =>
        {
            entity.HasKey(b => b.LibraryId);
            entity.HasIndex(b => b.StudentId).IsUnique();
        });

        builder.Entity<LibraryTag>(entity =>
        {
            entity.HasKey(t => t.Name);
        });
    }
}
