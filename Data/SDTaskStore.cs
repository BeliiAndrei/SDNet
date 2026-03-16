using SDNet.Models;
using SDNet.Services.TaskCreation;
using SDNet.Services.TaskStatusAudit;

namespace SDNet.Data
{
    public class SDTaskStore : ISDTaskStore
    {
        private readonly object _lock = new();
        private readonly List<SDTask> _tasks = [];
        private readonly ISDTaskFactoryMethodService _taskFactoryMethodService;
        private readonly TaskStatusChangeAuditComponent _taskStatusChangeAuditComponent;

        public SDTaskStore(
            ISDTaskFactoryMethodService taskFactoryMethodService,
            TaskStatusChangeAuditComponent taskStatusChangeAuditComponent)
        {
            _taskFactoryMethodService = taskFactoryMethodService;
            _taskStatusChangeAuditComponent = taskStatusChangeAuditComponent;
        }

        public IReadOnlyList<SDTask> GetAll()
        {
            lock (_lock)
            {
                return _tasks.ToList();
            }
        }

        public Task<IReadOnlyList<SDTask>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(GetAll());
        }

        public SDTask CreateNew(string taskTypeName)
        {
            lock (_lock)
            {
                SDTask task = _taskFactoryMethodService.CreateTask(taskTypeName);
                FillDefaults(task);
                _tasks.Add(task);
                return task;
            }
        }

        public Task<SDTask> CreateNewAsync(string taskTypeName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(CreateNew(taskTypeName));
        }

        public SDTask Clone(Guid id)
        {
            lock (_lock)
            {
                SDTask original = _tasks.FirstOrDefault(t => t.Id == id)
                    ?? throw new InvalidOperationException("Task not found.");

                SDTask clone = (SDTask)original.Clone();
                clone.Id = Guid.NewGuid();
                clone.UserQueryId = PeekNextUserQueryIdInternal();
                clone.DateReg = DateTime.Now;
                clone.StateName = "Новая";
                clone.DateClosed = null;
                clone.PerformPercent = 0;
                clone.ShortDescription = $"{original.ShortDescription} (копия)";
                _tasks.Add(clone);
                return clone;
            }
        }

        public Task<SDTask> CloneAsync(Guid id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Clone(id));
        }

        public SDTask? GetById(Guid id)
        {
            lock (_lock)
            {
                return _tasks.FirstOrDefault(t => t.Id == id);
            }
        }

        public Task<SDTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(GetById(id));
        }

        public int PeekNextUserQueryId()
        {
            lock (_lock)
            {
                return PeekNextUserQueryIdInternal();
            }
        }

        public Task<int> PeekNextUserQueryIdAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(PeekNextUserQueryId());
        }

        public void Save(SDTask task)
        {
            TaskStatusChangeAuditRecord? auditRecord = null;

            lock (_lock)
            {
                SDTask? existingTask = _tasks.FirstOrDefault(t => t.Id == task.Id);

                if (task.Id == Guid.Empty)
                {
                    task.Id = Guid.NewGuid();
                }

                if (task.UserQueryId <= 0)
                {
                    task.UserQueryId = PeekNextUserQueryIdInternal();
                }

                if (task.DateReg == default)
                {
                    task.DateReg = DateTime.Now;
                }

                int idx = _tasks.FindIndex(t => t.Id == task.Id);
                if (idx >= 0)
                {
                    _tasks[idx] = task;
                }
                else
                {
                    _tasks.Add(task);
                }

                if (existingTask is not null &&
                    !string.Equals(existingTask.StateName, task.StateName, StringComparison.OrdinalIgnoreCase))
                {
                    auditRecord = new TaskStatusChangeAuditRecord
                    {
                        TaskId = task.Id,
                        UserQueryId = task.UserQueryId,
                        OldStateName = existingTask.StateName,
                        NewStateName = task.StateName
                    };
                }
            }

            if (auditRecord is not null)
            {
                _taskStatusChangeAuditComponent.Save(auditRecord);
            }
        }

        public Task SaveAsync(SDTask task, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Save(task);
            return Task.CompletedTask;
        }

        public void Delete(Guid id)
        {
            lock (_lock)
            {
                int idx = _tasks.FindIndex(t => t.Id == id);
                if (idx >= 0)
                {
                    _tasks.RemoveAt(idx);
                }
            }
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Delete(id);
            return Task.CompletedTask;
        }

        private int PeekNextUserQueryIdInternal()
        {
            return _tasks.Count == 0 ? 120001 : _tasks.Max(t => t.UserQueryId) + 1;
        }

        private void FillDefaults(SDTask task)
        {
            DateTime now = DateTime.Now;
            task.Id = Guid.NewGuid();
            task.UserQueryId = PeekNextUserQueryIdInternal();
            task.DateReg = now;
            task.Priority = "Средний";
            task.UserFio = "";
            task.UserDepartName = "Service Desk";
            task.UserQueryTag = "NEW";
            task.QueryTypeName = "Запрос на обслуживание";
            task.ItProjectName = "SDNet";
            task.ShortDescription = "";
            task.StateName = "Новая";
            task.DateNeedClose = now.AddDays(2);
            task.PerformerName = "Не назначен";
            task.PerformerDepartName = "Service Desk";
            task.PerformPercent = 0;
            task.DateClosed = null;
        }
    }
}
