using iBorrow.Data;
using iBorrow.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace iBorrow.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _db.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<User> RegisterAsync(string email, string password, bool isAdmin = false)
    {
        var user = new User { Email = email, IsAdmin = isAdmin };
        user.Password = _passwordHasher.HashPassword(user, password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<User?> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        if (user is null)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
        return result == PasswordVerificationResult.Failed ? null : user;
    }

    public async Task EnsureSeedAccountsAsync()
    {
        if (!await EmailExistsAsync("user@test.com"))
        {
            await RegisterAsync("user@test.com", "password", isAdmin: false);
        }

        if (!await EmailExistsAsync("admin@test.com"))
        {
            await RegisterAsync("admin@test.com", "password", isAdmin: true);
        }
    }
}
