using SDNet.Models;

namespace SDNet.Services.TaskWorkflow
{
    public sealed class TaskWorkflowContext
    {
        private readonly ITaskStateFactory _taskStateFactory;

        public TaskWorkflowContext(
            SDTask task,
            UserInfo? currentUser,
            ITaskStateFactory taskStateFactory)
        {
            Task = task ?? throw new ArgumentNullException(nameof(task));
            CurrentUser = currentUser;
            _taskStateFactory = taskStateFactory ?? throw new ArgumentNullException(nameof(taskStateFactory));
            State = _taskStateFactory.Create(TaskStateCatalog.Normalize(task.StateId));
        }

        public SDTask Task { get; }

        public UserInfo? CurrentUser { get; }

        public TaskState State { get; private set; }

        public void TransitionTo(TaskStateCode targetState)
        {
            State.Handle(this, targetState);
        }

        public void RestoreTo(TaskStateCode targetState)
        {
            State.Restore(this, targetState);
        }

        public IReadOnlyList<TaskStateOption> GetAvailableStates(bool includeCurrent = true)
        {
            var options = new List<TaskStateOption>();
            if (includeCurrent)
            {
                options.Add(new TaskStateOption(State.Code, State.DisplayName));
            }

            foreach (TaskStateCode code in State.GetAvailableTransitions())
            {
                if (options.Any(option => option.Code == code))
                {
                    continue;
                }

                options.Add(new TaskStateOption(code, TaskStateCatalog.GetName(code)));
            }

            return options;
        }

        internal void SetState(TaskStateCode targetState)
        {
            State = _taskStateFactory.Create(targetState);
            Task.StateId = (int)targetState;
            State.OnEnter(this);
        }
    }
}
