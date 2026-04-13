namespace SDNet.Models
{
    public sealed class TaskStateOption
    {
        public TaskStateOption(TaskStateCode code, string displayName)
        {
            Code = code;
            DisplayName = displayName;
        }

        public TaskStateCode Code { get; }

        public string DisplayName { get; }

        public override string ToString() => DisplayName;
    }
}
