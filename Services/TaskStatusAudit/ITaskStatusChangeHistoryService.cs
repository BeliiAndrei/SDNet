using SDNet.Models;

namespace SDNet.Services.TaskStatusAudit
{
    public interface ITaskStatusChangeHistoryService
    {
        IReadOnlyList<TaskStatusChangeHistoryItem> GetHistory(int? userQueryId = null);
    }
}
