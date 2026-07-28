using iBorrow.Models;

namespace iBorrow.Services;

public static class BorrowerValidation
{
    public static bool IsValid(BorrowerProfile item) =>
        !string.IsNullOrWhiteSpace(item.StudentId) && !string.IsNullOrWhiteSpace(item.Name) &&
        item.Name.Count(c => c == ',') == 1 && item.Name.Split(',', 2).All(part => !string.IsNullOrWhiteSpace(part)) &&
        !string.IsNullOrWhiteSpace(item.ContactNo) && !string.IsNullOrWhiteSpace(item.Email) &&
        IsEmail(item.Email);

    public static bool IsEmail(string email)
    {
        try { return new System.Net.Mail.MailAddress(email).Address == email; }
        catch (FormatException) { return false; }
    }
}
