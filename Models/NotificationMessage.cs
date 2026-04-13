namespace SDNet.Models
{
    public sealed class NotificationMessage
    {
        public long Id { get; set; }

        public Guid TaskId { get; set; }

        public int? UserQueryId { get; set; }

        public string RecipientLogin { get; set; } = string.Empty;

        public string RecipientName { get; set; } = string.Empty;

        public string RecipientEmail { get; set; } = string.Empty;

        public string Channel { get; set; } = string.Empty;

        public string EventType { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string CreatedByLogin { get; set; } = string.Empty;

        public string CreatedByName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? SentAt { get; set; }
    }
}
