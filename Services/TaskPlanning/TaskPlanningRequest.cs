using SDNet.Models;

namespace SDNet.Services.TaskPlanning
{
    public sealed class TaskPlanningRequest
    {
        public string TaskTypeName { get; init; } = string.Empty;

        public DateTime RegisteredAt { get; init; }

        public string CurrentPriority { get; init; } = string.Empty;

        public string CurrentPerformerDepartment { get; init; } = string.Empty;

        public UserInfo? CurrentUser { get; init; }
    }
}
