using SDNet.Models;
using SDNet.Services.TaskStatusAudit;

namespace SDNet.Services.TaskEvents
{
    public sealed class TaskStatusHistoryCacheObserver : ITaskObserver, ITaskStatusChangeHistoryService
    {
        private readonly object _sync = new();
        private readonly SqlTaskStatusChangeHistoryService _source;
        private List<TaskStatusChangeHistoryItem>? _cache;

        public TaskStatusHistoryCacheObserver(SqlTaskStatusChangeHistoryService source)
        {
            _source = source;
        }

        public IReadOnlyList<TaskStatusChangeHistoryItem> GetHistory(int? userQueryId = null)
        {
            EnsureLoaded();

            lock (_sync)
            {
                IEnumerable<TaskStatusChangeHistoryItem> query = _cache!;
                if (userQueryId.HasValue)
                {
                    query = query.Where(item => item.UserQueryId == userQueryId.Value);
                }

                return query
                    .OrderByDescending(item => item.ChangedAt)
                    .ThenByDescending(item => item.Id)
                    .Select(Clone)
                    .ToList();
            }
        }

        public void Update(TaskDomainEvent domainEvent)
        {
            if (domainEvent is not TaskStatusChangedDomainEvent statusChanged || statusChanged.PreviousTask is null)
            {
                return;
            }

            EnsureLoaded();

            lock (_sync)
            {
                long nextId = _cache!.Count == 0 ? 1 : _cache.Max(item => item.Id) + 1;
                _cache.Add(new TaskStatusChangeHistoryItem
                {
                    Id = nextId,
                    TaskId = statusChanged.Task.Id,
                    UserQueryId = statusChanged.Task.UserQueryId,
                    OldStateId = statusChanged.PreviousTask.StateId,
                    OldStateName = statusChanged.PreviousTask.StateName,
                    NewStateId = statusChanged.Task.StateId,
                    NewStateName = statusChanged.Task.StateName,
                    ChangedByLogin = statusChanged.InitiatedBy?.UserName ?? string.Empty,
                    ChangedByName = statusChanged.InitiatedBy?.UserFullName ?? string.Empty,
                    ChangedAt = statusChanged.OccurredAt
                });
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
                _cache ??= _source.GetHistory().Select(Clone).ToList();
            }
        }

        private static TaskStatusChangeHistoryItem Clone(TaskStatusChangeHistoryItem item)
        {
            return new TaskStatusChangeHistoryItem
            {
                Id = item.Id,
                TaskId = item.TaskId,
                UserQueryId = item.UserQueryId,
                OldStateId = item.OldStateId,
                OldStateName = item.OldStateName,
                NewStateId = item.NewStateId,
                NewStateName = item.NewStateName,
                ChangedByLogin = item.ChangedByLogin,
                ChangedByName = item.ChangedByName,
                ChangedAt = item.ChangedAt
            };
        }
    }
}
