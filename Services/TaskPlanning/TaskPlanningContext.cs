namespace SDNet.Services.TaskPlanning
{
    public sealed class TaskPlanningContext
    {
        public TaskPlanningContext(ITaskPlanningStrategy strategy)
        {
            Strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        public ITaskPlanningStrategy Strategy { get; private set; }

        public void SetStrategy(ITaskPlanningStrategy strategy)
        {
            Strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        public TaskPlanningResult BuildPlan(TaskPlanningRequest request)
        {
            return Strategy.BuildPlan(request);
        }
    }
}
