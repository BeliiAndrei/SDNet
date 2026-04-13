namespace SDNet.Services.TaskOperations
{
    public sealed class TaskOperationResult
    {
        public static TaskOperationResult Success() => new(true, string.Empty);

        public static TaskOperationResult Fail(string message) => new(false, message);

        private TaskOperationResult(bool isSuccessful, string message)
        {
            IsSuccessful = isSuccessful;
            Message = message;
        }

        public bool IsSuccessful { get; }

        public string Message { get; }
    }
}
