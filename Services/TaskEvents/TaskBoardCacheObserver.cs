using SDNet.Data;
using SDNet.Models;

namespace SDNet.Services.TaskEvents
{
    public interface ITaskBoardReadService
    {
        IReadOnlyList<SDTask> GetAll();
        SDTask? GetById(Guid id);
        IReadOnlyList<SDTask> GetAssignedTo(string performerName);
        IReadOnlyList<SDTask> GetOverdue();
    }

    public sealed class TaskBoardCacheObserver : ITaskObserver, ITaskBoardReadService
    {
        private readonly object _sync = new();
        private readonly ISDTaskStore _taskStore;
        private List<SDTask>? _cache;

        public TaskBoardCacheObserver(ISDTaskStore taskStore)
        {
            _taskStore = taskStore;
        }

        public IReadOnlyList<SDTask> GetAll()
        {
            EnsureLoaded();
            lock (_sync)
            {
                return _cache!.Select(CloneTask).ToList();
            }
        }

        public SDTask? GetById(Guid id)
        {
            EnsureLoaded();
            lock (_sync)
            {
                SDTask? task = _cache!.FirstOrDefault(item => item.Id == id);
                return task is null ? null : CloneTask(task);
            }
        }

        public IReadOnlyList<SDTask> GetAssignedTo(string performerName)
        {
            EnsureLoaded();
            lock (_sync)
            {
                return _cache!
                    .Where(task => string.Equals(task.PerformerName, performerName, StringComparison.OrdinalIgnoreCase))
                    .Select(CloneTask)
                    .ToList();
            }
        }

        public IReadOnlyList<SDTask> GetOverdue()
        {
            EnsureLoaded();
            lock (_sync)
            {
                DateTime today = DateTime.Today;
                return _cache!
                    .Where(task => task.DateClosed is null && task.DateNeedClose.Date < today)
                    .Select(CloneTask)
                    .ToList();
            }
        }

        public void Update(TaskDomainEvent domainEvent)
        {
            EnsureLoaded();
            lock (_sync)
            {
                if (domainEvent is TaskDeletedDomainEvent deleted)
                {
                    _cache!.RemoveAll(task => task.Id == deleted.Task.Id);
                    return;
                }

                SDTask task = CloneTask(domainEvent.Task);
                int index = _cache!.FindIndex(item => item.Id == task.Id);
                if (index >= 0)
                {
                    _cache[index] = task;
                }
                else
                {
                    _cache.Add(task);
                }
            }
        }

        private void EnsureLoaded()
        {
            if (_cache is not null)
            {
                return;
            }

            lock (_sync)
            {
                _cache ??= _taskStore.GetAll().Select(CloneTask).ToList();
            }
        }

        private static SDTask CloneTask(SDTask task)
        {
            return (SDTask)task.Clone();
        }
    }
}
