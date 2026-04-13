namespace SDNet.Services.TaskEvents
{
    public interface ITaskObserver
    {
        void Update(TaskDomainEvent domainEvent);
    }

    public interface ITaskEventSubject
    {
        void Attach(ITaskObserver observer);
        void Detach(ITaskObserver observer);
        void Notify(TaskDomainEvent domainEvent);
    }

    public sealed class TaskEventSubject : ITaskEventSubject
    {
        private readonly List<ITaskObserver> _observers;

        public TaskEventSubject(IEnumerable<ITaskObserver> observers)
        {
            _observers = observers.ToList();
        }

        public void Attach(ITaskObserver observer)
        {
            if (_observers.Contains(observer))
            {
                return;
            }

            _observers.Add(observer);
        }

        public void Detach(ITaskObserver observer)
        {
            _observers.Remove(observer);
        }

        public void Notify(TaskDomainEvent domainEvent)
        {
            foreach (ITaskObserver observer in _observers)
            {
                observer.Update(domainEvent);
            }
        }
    }
}
