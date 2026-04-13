using SDNet.Models;

namespace SDNet.Services.TaskWorkflow
{
    public abstract class TaskState
    {
        public abstract TaskStateCode Code { get; }

        public string DisplayName => TaskStateCatalog.GetName(Code);

        public abstract IReadOnlyCollection<TaskStateCode> GetAvailableTransitions();

        public virtual void Handle(TaskWorkflowContext context, TaskStateCode targetState)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (targetState == Code)
            {
                OnEnter(context);
                return;
            }

            if (!GetAvailableTransitions().Contains(targetState))
            {
                throw new InvalidOperationException(
                    $"Переход из состояния \"{DisplayName}\" в \"{TaskStateCatalog.GetName(targetState)}\" недопустим.");
            }

            context.SetState(targetState);
        }

        public virtual void Restore(TaskWorkflowContext context, TaskStateCode targetState)
        {
            ArgumentNullException.ThrowIfNull(context);
            context.SetState(targetState);
        }

        public abstract void OnEnter(TaskWorkflowContext context);
    }
}
