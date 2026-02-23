using SDNet.Models;

namespace SDNet.Services.TaskCreation
{
    public interface ISDTaskFactoryMethodService
    {
        IReadOnlyList<string> SupportedTaskTypes { get; }

        SDTask CreateTask(string? taskTypeName);
    }

    public abstract class SDTaskCreator
    {
        protected SDTaskCreator(string taskTypeName)
        {
            if (string.IsNullOrWhiteSpace(taskTypeName))
            {
                throw new ArgumentException("Task type name must be provided.", nameof(taskTypeName));
            }

            TaskTypeName = taskTypeName;
        }

        public string TaskTypeName { get; }

        public abstract SDTask FactoryMethod();

        public virtual bool CanHandle(string? taskTypeName)
        {
            if (string.IsNullOrWhiteSpace(taskTypeName))
            {
                return false;
            }

            return string.Equals(taskTypeName.Trim(), TaskTypeName, StringComparison.OrdinalIgnoreCase);
        }

        protected static bool Contains(string? taskTypeName, string token)
        {
            if (string.IsNullOrWhiteSpace(taskTypeName) || string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            return taskTypeName.Contains(token, StringComparison.OrdinalIgnoreCase);
        }
    }

    public sealed class ITTaskCreator : SDTaskCreator
    {
        public ITTaskCreator()
            : base(SDTaskTypes.ITTask)
        {
        }

        public override SDTask FactoryMethod() => new ITTask();

        public override bool CanHandle(string? taskTypeName)
        {
            return base.CanHandle(taskTypeName) ||
                   Contains(taskTypeName, "it task") ||
                   Contains(taskTypeName, "service desk");
        }
    }

    public sealed class HardwareTaskCreator : SDTaskCreator
    {
        public HardwareTaskCreator()
            : base(SDTaskTypes.HardwareTask)
        {
        }

        public override SDTask FactoryMethod() => new HardwareTask();

        public override bool CanHandle(string? taskTypeName)
        {
            return base.CanHandle(taskTypeName) ||
                   Contains(taskTypeName, "hardware") ||
                   Contains(taskTypeName, "оборуд");
        }
    }

    public sealed class CommunicationTaskCreator : SDTaskCreator
    {
        public CommunicationTaskCreator()
            : base(SDTaskTypes.CommunicationTask)
        {
        }

        public override SDTask FactoryMethod() => new CommunicationTask();

        public override bool CanHandle(string? taskTypeName)
        {
            return base.CanHandle(taskTypeName) ||
                   Contains(taskTypeName, "communication") ||
                   Contains(taskTypeName, "коммуник");
        }
    }

    public sealed class AccessTaskCreator : SDTaskCreator
    {
        public AccessTaskCreator()
            : base(SDTaskTypes.AccessTask)
        {
        }

        public override SDTask FactoryMethod() => new AccessTask();

        public override bool CanHandle(string? taskTypeName)
        {
            return base.CanHandle(taskTypeName) ||
                   Contains(taskTypeName, "access");
        }
    }

    public sealed class SecurityTaskCreator : SDTaskCreator
    {
        public SecurityTaskCreator()
            : base(SDTaskTypes.SecurityTask)
        {
        }

        public override SDTask FactoryMethod() => new SecurityTask();

        public override bool CanHandle(string? taskTypeName)
        {
            return base.CanHandle(taskTypeName) ||
                   Contains(taskTypeName, "security") ||
                   Contains(taskTypeName, "безопас");
        }
    }

    public sealed class IntegrationTaskCreator : SDTaskCreator
    {
        public IntegrationTaskCreator()
            : base(SDTaskTypes.IntegrationTask)
        {
        }

        public override SDTask FactoryMethod() => new IntegrationTask();

        public override bool CanHandle(string? taskTypeName)
        {
            return base.CanHandle(taskTypeName) ||
                   Contains(taskTypeName, "integration") ||
                   Contains(taskTypeName, "интеграц");
        }
    }

    public sealed class SDTaskFactoryMethodService : ISDTaskFactoryMethodService
    {
        private readonly IReadOnlyList<SDTaskCreator> _creators;
        private readonly SDTaskCreator _defaultCreator;

        public SDTaskFactoryMethodService(IEnumerable<SDTaskCreator> creators)
        {
            ArgumentNullException.ThrowIfNull(creators);

            List<SDTaskCreator> creatorList = creators.ToList();
            if (creatorList.Count == 0)
            {
                throw new InvalidOperationException("At least one task creator must be registered.");
            }

            _creators = creatorList;
            _defaultCreator = creatorList.FirstOrDefault(c => c is ITTaskCreator) ?? creatorList[0];
            SupportedTaskTypes = creatorList
                .Select(c => c.TaskTypeName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public IReadOnlyList<string> SupportedTaskTypes { get; }

        public SDTask CreateTask(string? taskTypeName)
        {
            SDTaskCreator creator = _creators.FirstOrDefault(c => c.CanHandle(taskTypeName)) ?? _defaultCreator;
            return creator.FactoryMethod();
        }
    }
}
