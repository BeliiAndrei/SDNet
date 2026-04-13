using SDNet.Models;

namespace SDNet.Services.TaskWorkflow
{
    public interface ITaskWorkflowService
    {
        IReadOnlyList<TaskStateOption> GetAvailableStates(SDTask task, UserInfo? currentUser, bool includeCurrent = true);
        void ApplyState(SDTask task, UserInfo? currentUser, TaskStateCode targetState);
        void RestoreState(SDTask task, UserInfo? currentUser, TaskStateCode targetState);
    }

    public sealed class TaskWorkflowService : ITaskWorkflowService
    {
        private readonly ITaskStateFactory _taskStateFactory;

        public TaskWorkflowService(ITaskStateFactory taskStateFactory)
        {
            _taskStateFactory = taskStateFactory;
        }

        public IReadOnlyList<TaskStateOption> GetAvailableStates(SDTask task, UserInfo? currentUser, bool includeCurrent = true)
        {
            var context = new TaskWorkflowContext(task, currentUser, _taskStateFactory);
            return context.GetAvailableStates(includeCurrent);
        }

        public void ApplyState(SDTask task, UserInfo? currentUser, TaskStateCode targetState)
        {
            var context = new TaskWorkflowContext(task, currentUser, _taskStateFactory);
            context.TransitionTo(targetState);
        }

        public void RestoreState(SDTask task, UserInfo? currentUser, TaskStateCode targetState)
        {
            var context = new TaskWorkflowContext(task, currentUser, _taskStateFactory);
            context.RestoreTo(targetState);
        }
    }
}
