namespace iBorrow.Models
{
    public class NotificationItem
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public bool IsUnread { get; set; }
    }
}
