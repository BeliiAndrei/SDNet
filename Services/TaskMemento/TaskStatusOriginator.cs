using SDNet.Models;

namespace SDNet.Services.TaskMemento
{
    public sealed class TaskStatusOriginator
    {
        private SDTask? _task;

        public void SetTask(SDTask task)
        {
            _task = task ?? throw new ArgumentNullException(nameof(task));
        }

        public TaskStatusMemento CreateMemento()
        {
            if (_task is null)
            {
                throw new InvalidOperationException("Задача для создания снимка не задана.");
            }

            return new TaskStatusMemento
            {
                TaskId = _task.Id,
                UserQueryId = _task.UserQueryId,
                StateId = _task.StateId,
                StateName = _task.StateName,
                PerformPercent = _task.PerformPercent,
                DateClosed = _task.DateClosed,
                CapturedAt = DateTime.Now
            };
        }

        public void Restore(SDTask task, TaskStatusMemento memento)
        {
            ArgumentNullException.ThrowIfNull(task);
            ArgumentNullException.ThrowIfNull(memento);

            task.StateId = memento.StateId;
            task.PerformPercent = memento.PerformPercent;
            task.DateClosed = memento.DateClosed;
        }
    }
}
