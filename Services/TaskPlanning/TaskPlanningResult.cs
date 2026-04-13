namespace SDNet.Services.TaskPlanning
{
    public sealed class TaskPlanningResult
    {
        public DateTime RecommendedDueDate { get; init; }

        public string RecommendedPriority { get; init; } = string.Empty;

        public string RecommendedPerformerDepartment { get; init; } = string.Empty;

        public string Note { get; init; } = string.Empty;
    }
}
