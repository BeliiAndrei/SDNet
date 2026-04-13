namespace SDNet.Services.TaskOperations
{
    public interface ITaskValidationPipeline
    {
        TaskOperationResult Validate(TaskSaveRequest request);
    }

    public sealed class TaskValidationPipeline : ITaskValidationPipeline
    {
        private readonly ITaskSaveHandler _head;

        public TaskValidationPipeline(ITaskSaveHandler head)
        {
            _head = head;
        }

        public TaskOperationResult Validate(TaskSaveRequest request)
        {
            return _head.Handle(request);
        }
    }
}
