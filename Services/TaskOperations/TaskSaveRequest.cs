using SDNet.Models;

namespace SDNet.Services.TaskOperations
{
    public sealed class TaskSaveRequest
    {
        public SDTask Task { get; init; } = null!;

        public SDTask? ExistingTask { get; init; }

        public UserInfo? CurrentUser { get; init; }

        public bool IsUndoOperation { get; init; }
    }
}
