using Microsoft.EntityFrameworkCore;
using iBorrow.Models;

namespace iBorrow.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { }

    public DbSet<User> Users => Set<User>();
}
