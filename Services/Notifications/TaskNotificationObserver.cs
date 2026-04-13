using SDNet.Models;
using SDNet.Services.Auth;
using SDNet.Services.TaskEvents;

namespace SDNet.Services.Notifications
{
    public sealed class TaskNotificationObserver : ITaskObserver, INotificationHistoryService
    {
        private readonly object _sync = new();
        private readonly SqlNotificationHistoryService _source;
        private readonly INotificationGateway _notificationGateway;
        private readonly IUserDirectoryService _userDirectoryService;
        private List<NotificationMessage>? _cache;

        public TaskNotificationObserver(
            SqlNotificationHistoryService source,
            INotificationGateway notificationGateway,
            IUserDirectoryService userDirectoryService)
        {
            _source = source;
            _notificationGateway = notificationGateway;
            _userDirectoryService = userDirectoryService;
        }

        public IReadOnlyList<NotificationMessage> GetAll()
        {
            EnsureLoaded();
            lock (_sync)
            {
                return _cache!
                    .OrderByDescending(item => item.CreatedAt)
                    .ThenByDescending(item => item.Id)
                    .Select(Clone)
                    .ToList();
            }
        }

        public void Save(NotificationMessage message)
        {
            _source.Save(message);
            EnsureLoaded();

            lock (_sync)
            {
                long nextId = _cache!.Count == 0 ? 1 : _cache.Max(item => item.Id) + 1;
                NotificationMessage copy = Clone(message);
                copy.Id = nextId;
                _cache.Add(copy);
            }
        }

        public void Update(TaskDomainEvent domainEvent)
        {
            switch (domainEvent)
            {
                case TaskAssignedDomainEvent assignedEvent:
                    PublishAssignmentNotification(assignedEvent);
                    break;
                case TaskStatusChangedDomainEvent statusChangedEvent:
                    PublishStatusChangedNotification(statusChangedEvent);
                    break;
            }
        }

        private void PublishAssignmentNotification(TaskAssignedDomainEvent domainEvent)
        {
            if (string.IsNullOrWhiteSpace(domainEvent.Task.PerformerName) ||
                string.Equals(domainEvent.Task.PerformerName, "Не назначен", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            UserInfo? recipient = _userDirectoryService.GetByFullName(domainEvent.Task.PerformerName);
            NotificationMessage message = CreateMessage(
                domainEvent,
                recipient,
                "Assignment",
                $"Назначена задача №{domainEvent.Task.UserQueryId}",
                $"Вам назначена задача \"{domainEvent.Task.ShortDescription}\". Текущий статус: {domainEvent.Task.StateName}.");

            Save(message);
            _notificationGateway.Send(message);
        }

        private void PublishStatusChangedNotification(TaskStatusChangedDomainEvent domainEvent)
        {
            if (string.IsNullOrWhiteSpace(domainEvent.Task.PerformerName) ||
                string.Equals(domainEvent.Task.PerformerName, "Не назначен", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            UserInfo? recipient = _userDirectoryService.GetByFullName(domainEvent.Task.PerformerName);
            NotificationMessage message = CreateMessage(
                domainEvent,
                recipient,
                "StatusChanged",
                $"Изменен статус задачи №{domainEvent.Task.UserQueryId}",
                $"Статус задачи \"{domainEvent.Task.ShortDescription}\" изменен: {domainEvent.PreviousTask!.StateName} -> {domainEvent.Task.StateName}.");

            Save(message);
            _notificationGateway.Send(message);
        }

        private static NotificationMessage CreateMessage(
            TaskDomainEvent domainEvent,
            UserInfo? recipient,
            string eventType,
            string subject,
            string body)
        {
            return new NotificationMessage
            {
                TaskId = domainEvent.Task.Id,
                UserQueryId = domainEvent.Task.UserQueryId,
                RecipientLogin = recipient?.UserName ?? string.Empty,
                RecipientName = recipient?.UserFullName ?? domainEvent.Task.PerformerName,
                RecipientEmail = recipient?.Email ?? string.Empty,
                Channel = "MockEmail",
                EventType = eventType,
                Subject = subject,
                Body = body,
                Status = "Sent",
                CreatedByLogin = domainEvent.InitiatedBy?.UserName ?? string.Empty,
                CreatedByName = domainEvent.InitiatedBy?.UserFullName ?? string.Empty,
                CreatedAt = domainEvent.OccurredAt,
                SentAt = domainEvent.OccurredAt
            };
        }

        private void EnsureLoaded()
        {
            if (_cache is not null)
            {
                return;
            }

            lock (_sync)
            {
                _cache ??= _source.GetAll().Select(Clone).ToList();
            }
        }

        private static NotificationMessage Clone(NotificationMessage source)
        {
            return new NotificationMessage
            {
                Id = source.Id,
                TaskId = source.TaskId,
                UserQueryId = source.UserQueryId,
                RecipientLogin = source.RecipientLogin,
                RecipientName = source.RecipientName,
                RecipientEmail = source.RecipientEmail,
                Channel = source.Channel,
                EventType = source.EventType,
                Subject = source.Subject,
                Body = source.Body,
                Status = source.Status,
                CreatedByLogin = source.CreatedByLogin,
                CreatedByName = source.CreatedByName,
                CreatedAt = source.CreatedAt,
                SentAt = source.SentAt
            };
        }
    }
}
