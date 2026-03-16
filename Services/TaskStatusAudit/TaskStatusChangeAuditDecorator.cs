namespace SDNet.Services.TaskStatusAudit
{
    public abstract class TaskStatusChangeAuditDecorator : TaskStatusChangeAuditComponent
    {
        private readonly TaskStatusChangeAuditComponent _component;

        protected TaskStatusChangeAuditDecorator(TaskStatusChangeAuditComponent component)
        {
            _component = component ?? throw new ArgumentNullException(nameof(component));
        }

        protected TaskStatusChangeAuditComponent Component => _component;

        public override void Save(TaskStatusChangeAuditRecord record)
        {
            _component.Save(record);
        }

        public override Task SaveAsync(
            TaskStatusChangeAuditRecord record,
            CancellationToken cancellationToken = default)
        {
            return _component.SaveAsync(record, cancellationToken);
        }
    }
}
