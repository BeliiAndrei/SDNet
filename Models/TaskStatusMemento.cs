namespace SDNet.Models
{
    public sealed class TaskStatusMemento
    {
        public Guid TaskId { get; set; }

        public int UserQueryId { get; set; }

        public int StateId { get; set; }

        public string StateName { get; set; } = string.Empty;

        public int PerformPercent { get; set; }

        public DateTime? DateClosed { get; set; }

        public DateTime CapturedAt { get; set; }
    }
}
