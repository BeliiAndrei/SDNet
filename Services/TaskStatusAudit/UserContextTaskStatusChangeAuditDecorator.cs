using SDNet.Models;

namespace SDNet.Services.TaskStatusAudit
{
    public sealed class UserContextTaskStatusChangeAuditDecorator : TaskStatusChangeAuditDecorator
    {
        private readonly CurrentUserContext _currentUserContext;

        public UserContextTaskStatusChangeAuditDecorator(
            TaskStatusChangeAuditComponent component,
            CurrentUserContext currentUserContext)
            : base(component)
        {
            _currentUserContext = currentUserContext ?? throw new ArgumentNullException(nameof(currentUserContext));
        }

        public override void Save(TaskStatusChangeAuditRecord record)
        {
            PopulateMissingContext(record);
            base.Save(record);
        }

        public override Task SaveAsync(
            TaskStatusChangeAuditRecord record,
            CancellationToken cancellationToken = default)
        {
            PopulateMissingContext(record);
            return base.SaveAsync(record, cancellationToken);
        }

        private void PopulateMissingContext(TaskStatusChangeAuditRecord record)
        {
            ArgumentNullException.ThrowIfNull(record);

            UserInfo? currentUser = _currentUserContext.CurrentUser;
            if (string.IsNullOrWhiteSpace(record.ChangedByLogin))
            {
                record.ChangedByLogin = currentUser?.UserName ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(record.ChangedByName))
            {
                record.ChangedByName = currentUser?.UserFullName ?? string.Empty;
            }

            if (record.ChangedAt == default)
            {
                record.ChangedAt = DateTime.Now;
            }
        }
    }
}
