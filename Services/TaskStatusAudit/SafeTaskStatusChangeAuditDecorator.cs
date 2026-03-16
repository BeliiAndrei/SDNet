using System.Diagnostics;

namespace SDNet.Services.TaskStatusAudit
{
    public sealed class SafeTaskStatusChangeAuditDecorator : TaskStatusChangeAuditDecorator
    {
        public SafeTaskStatusChangeAuditDecorator(TaskStatusChangeAuditComponent component)
            : base(component)
        {
        }

        public override void Save(TaskStatusChangeAuditRecord record)
        {
            try
            {
                base.Save(record);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TaskStatusAudit] {ex.Message}");
            }
        }

        public override async Task SaveAsync(
            TaskStatusChangeAuditRecord record,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await base.SaveAsync(record, cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TaskStatusAudit] {ex.Message}");
            }
        }
    }
}
