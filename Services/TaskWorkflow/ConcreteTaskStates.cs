using SDNet.Models;

namespace SDNet.Services.TaskWorkflow
{
    public sealed class NewTaskState : TaskState
    {
        public override TaskStateCode Code => TaskStateCode.New;

        public override IReadOnlyCollection<TaskStateCode> GetAvailableTransitions()
        {
            return [TaskStateCode.InProgress, TaskStateCode.Closed];
        }

        public override void OnEnter(TaskWorkflowContext context)
        {
            context.Task.DateClosed = null;
            context.Task.PerformPercent = 0;
        }
    }

    public sealed class InProgressTaskState : TaskState
    {
        public override TaskStateCode Code => TaskStateCode.InProgress;

        public override IReadOnlyCollection<TaskStateCode> GetAvailableTransitions()
        {
            return [TaskStateCode.Approval, TaskStateCode.Closed];
        }

        public override void OnEnter(TaskWorkflowContext context)
        {
            context.Task.DateClosed = null;
            if (context.Task.PerformPercent <= 0)
            {
                context.Task.PerformPercent = 10;
            }
        }
    }

    public sealed class ApprovalTaskState : TaskState
    {
        public override TaskStateCode Code => TaskStateCode.Approval;

        public override IReadOnlyCollection<TaskStateCode> GetAvailableTransitions()
        {
            return [TaskStateCode.InProgress, TaskStateCode.Closed];
        }

        public override void OnEnter(TaskWorkflowContext context)
        {
            context.Task.DateClosed = null;
            if (context.Task.PerformPercent < 90)
            {
                context.Task.PerformPercent = 90;
            }
        }
    }

    public sealed class ClosedTaskState : TaskState
    {
        public override TaskStateCode Code => TaskStateCode.Closed;

        public override IReadOnlyCollection<TaskStateCode> GetAvailableTransitions()
        {
            return [TaskStateCode.InProgress];
        }

        public override void OnEnter(TaskWorkflowContext context)
        {
            context.Task.PerformPercent = 100;
            context.Task.DateClosed ??= DateTime.Now;
        }
    }
}
