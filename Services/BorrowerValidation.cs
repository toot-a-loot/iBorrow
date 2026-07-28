using iBorrow.Models;

namespace iBorrow.Services;

public static class BorrowerValidation
{
    public static bool IsValid(BorrowerProfile item) =>
        !string.IsNullOrWhiteSpace(item.StudentId) && !string.IsNullOrWhiteSpace(item.Name) &&
        !string.IsNullOrWhiteSpace(item.Email) && IsEmail(item.Email);

    public static bool IsEmail(string email)
    {
        try { return new System.Net.Mail.MailAddress(email).Address == email; }
        catch (FormatException) { return false; }
    }
}
