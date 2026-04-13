namespace SDNet.Services.TaskPlanning
{
    public interface ITaskPlanningService
    {
        TaskPlanningResult BuildPlan(TaskPlanningRequest request);
    }

    public sealed class TaskPlanningService : ITaskPlanningService
    {
        private readonly IReadOnlyList<ITaskPlanningStrategy> _strategies;

        public TaskPlanningService(IEnumerable<ITaskPlanningStrategy> strategies)
        {
            _strategies = strategies.ToList();
        }

        public TaskPlanningResult BuildPlan(TaskPlanningRequest request)
        {
            ITaskPlanningStrategy strategy = _strategies.First(strategy => strategy.CanHandle(request.TaskTypeName));
            var context = new TaskPlanningContext(strategy);
            return context.BuildPlan(request);
        }
    }
}
