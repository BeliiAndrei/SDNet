using SDNet.Models;

namespace SDNet.Services.TaskPlanning
{
    public interface ITaskPlanningStrategy
    {
        bool CanHandle(string taskTypeName);
        TaskPlanningResult BuildPlan(TaskPlanningRequest request);
    }

    public sealed class DefaultTaskPlanningStrategy : ITaskPlanningStrategy
    {
        public bool CanHandle(string taskTypeName) => true;

        public TaskPlanningResult BuildPlan(TaskPlanningRequest request)
        {
            return new TaskPlanningResult
            {
                RecommendedDueDate = request.RegisteredAt.Date.AddDays(2),
                RecommendedPriority = string.IsNullOrWhiteSpace(request.CurrentPriority) ? "Средний" : request.CurrentPriority,
                RecommendedPerformerDepartment = string.IsNullOrWhiteSpace(request.CurrentPerformerDepartment)
                    ? request.CurrentUser?.UserDepartName ?? "Service Desk"
                    : request.CurrentPerformerDepartment,
                Note = "Базовая стратегия: стандартный SLA 2 дня."
            };
        }
    }

    public sealed class SecurityTaskPlanningStrategy : ITaskPlanningStrategy
    {
        public bool CanHandle(string taskTypeName)
        {
            return string.Equals(taskTypeName, SDTaskTypes.SecurityTask, StringComparison.OrdinalIgnoreCase);
        }

        public TaskPlanningResult BuildPlan(TaskPlanningRequest request)
        {
            return new TaskPlanningResult
            {
                RecommendedDueDate = request.RegisteredAt.Date.AddDays(1),
                RecommendedPriority = "Высокий",
                RecommendedPerformerDepartment = "Security",
                Note = "Стратегия ИБ: повышенный приоритет и SLA 1 день."
            };
        }
    }

    public sealed class IntegrationTaskPlanningStrategy : ITaskPlanningStrategy
    {
        public bool CanHandle(string taskTypeName)
        {
            return string.Equals(taskTypeName, SDTaskTypes.IntegrationTask, StringComparison.OrdinalIgnoreCase);
        }

        public TaskPlanningResult BuildPlan(TaskPlanningRequest request)
        {
            return new TaskPlanningResult
            {
                RecommendedDueDate = request.RegisteredAt.Date.AddDays(3),
                RecommendedPriority = "Высокий",
                RecommendedPerformerDepartment = "Integration",
                Note = "Стратегия интеграций: SLA 3 дня и маршрут в интеграционную команду."
            };
        }
    }

    public sealed class HardwareTaskPlanningStrategy : ITaskPlanningStrategy
    {
        public bool CanHandle(string taskTypeName)
        {
            return string.Equals(taskTypeName, SDTaskTypes.HardwareTask, StringComparison.OrdinalIgnoreCase);
        }

        public TaskPlanningResult BuildPlan(TaskPlanningRequest request)
        {
            return new TaskPlanningResult
            {
                RecommendedDueDate = request.RegisteredAt.Date.AddDays(5),
                RecommendedPriority = "Средний",
                RecommendedPerformerDepartment = "Support",
                Note = "Стратегия оборудования: SLA 5 дней с маршрутом в поддержку."
            };
        }
    }
}
