namespace SDNet.Services.TaskStatusAudit
{
    public abstract class TaskStatusChangeAuditComponent
    {
        public abstract void Save(TaskStatusChangeAuditRecord record);

        public virtual Task SaveAsync(
            TaskStatusChangeAuditRecord record,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Save(record);
            return Task.CompletedTask;
        }
    }
}
