using SDNet.Models;

namespace SDNet.Services.TaskWorkflow
{
    public interface ITaskStateFactory
    {
        TaskState Create(TaskStateCode stateCode);
    }

    public sealed class TaskStateFactory : ITaskStateFactory
    {
        private readonly IReadOnlyDictionary<TaskStateCode, TaskState> _states;

        public TaskStateFactory(IEnumerable<TaskState> states)
        {
            _states = states.ToDictionary(state => state.Code);
        }

        public TaskState Create(TaskStateCode stateCode)
        {
            return _states.TryGetValue(stateCode, out TaskState? state)
                ? state
                : _states[TaskStateCode.New];
        }
    }
}
