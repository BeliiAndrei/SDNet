using SDNet.Models;

namespace SDNet.Services.TaskEvents
{
    public abstract class TaskDomainEvent
    {
        protected TaskDomainEvent(SDTask task, SDTask? previousTask, UserInfo? initiatedBy)
        {
            Task = task;
            PreviousTask = previousTask;
            InitiatedBy = initiatedBy;
            OccurredAt = DateTime.Now;
        }

        public SDTask Task { get; }

        public SDTask? PreviousTask { get; }

        public UserInfo? InitiatedBy { get; }

        public DateTime OccurredAt { get; }
    }

    public sealed class TaskSavedDomainEvent : TaskDomainEvent
    {
        public TaskSavedDomainEvent(SDTask task, SDTask? previousTask, UserInfo? initiatedBy)
            : base(task, previousTask, initiatedBy)
        {
        }
    }

    public sealed class TaskDeletedDomainEvent : TaskDomainEvent
    {
        public TaskDeletedDomainEvent(SDTask task, UserInfo? initiatedBy)
            : base(task, task, initiatedBy)
        {
        }
    }

    public sealed class TaskStatusChangedDomainEvent : TaskDomainEvent
    {
        public TaskStatusChangedDomainEvent(SDTask task, SDTask previousTask, UserInfo? initiatedBy)
            : base(task, previousTask, initiatedBy)
        {
        }
    }

    public sealed class TaskAssignedDomainEvent : TaskDomainEvent
    {
        public TaskAssignedDomainEvent(SDTask task, SDTask? previousTask, UserInfo? initiatedBy)
            : base(task, previousTask, initiatedBy)
        {
        }
    }
}
