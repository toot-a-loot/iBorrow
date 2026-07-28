using iBorrow.Models;

namespace iBorrow.Services
{
    public static class NotificationsStore
    {
        private static readonly List<NotificationItem> All = new()
        {
            new() { Title = "Book Due Soon", Message = "\"1984\" by George Orwell is due in 2 days.", TimeAgo = "2h ago", IsUnread = true },
            new() { Title = "Reservation Ready", Message = "Your reserved copy of \"Dune\" is ready for pickup.", TimeAgo = "5h ago", IsUnread = true },
            new() { Title = "Book Returned", Message = "Thank you for returning \"The Hobbit\" on time.", TimeAgo = "1d ago", IsUnread = false },
            new() { Title = "Overdue Notice", Message = "\"Sapiens\" is overdue. Please return it as soon as possible.", TimeAgo = "3d ago", IsUnread = false },
            new() { Title = "New Arrival", Message = "\"Project Hail Mary\" has been added to the library catalog.", TimeAgo = "6d ago", IsUnread = false },
        };

        public static IReadOnlyList<NotificationItem> GetAll() => All;

        public static IReadOnlyList<NotificationItem> GetPreview(int count = 4) => All.Take(count).ToList();
    }
}
