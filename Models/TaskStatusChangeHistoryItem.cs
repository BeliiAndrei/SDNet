namespace SDNet.Models
{
    public sealed class TaskStatusChangeHistoryItem
    {
        public long Id { get; set; }

        public Guid TaskId { get; set; }

        public int UserQueryId { get; set; }

        public string OldStateName { get; set; } = string.Empty;

        public string NewStateName { get; set; } = string.Empty;

        public string ChangedByLogin { get; set; } = string.Empty;

        public string ChangedByName { get; set; } = string.Empty;

        public DateTime ChangedAt { get; set; }
    }
}
