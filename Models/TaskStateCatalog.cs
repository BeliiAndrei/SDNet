namespace SDNet.Models
{
    public static class TaskStateCatalog
    {
        private static readonly IReadOnlyDictionary<TaskStateCode, string> Names = new Dictionary<TaskStateCode, string>
        {
            [TaskStateCode.New] = "Новая",
            [TaskStateCode.InProgress] = "В работе",
            [TaskStateCode.Approval] = "Согласование",
            [TaskStateCode.Closed] = "Закрыта"
        };

        private static readonly IReadOnlyDictionary<string, TaskStateCode> CodesByName =
            Names.ToDictionary(pair => pair.Value, pair => pair.Key, StringComparer.OrdinalIgnoreCase);

        public static IReadOnlyList<TaskStateOption> AllOptions { get; } =
        [
            new TaskStateOption(TaskStateCode.New, Names[TaskStateCode.New]),
            new TaskStateOption(TaskStateCode.InProgress, Names[TaskStateCode.InProgress]),
            new TaskStateOption(TaskStateCode.Approval, Names[TaskStateCode.Approval]),
            new TaskStateOption(TaskStateCode.Closed, Names[TaskStateCode.Closed])
        ];

        public static string GetName(TaskStateCode code)
        {
            return Names.TryGetValue(code, out string? name)
                ? name
                : Names[TaskStateCode.New];
        }

        public static TaskStateCode Normalize(int value)
        {
            return Enum.IsDefined(typeof(TaskStateCode), value)
                ? (TaskStateCode)value
                : TaskStateCode.New;
        }

        public static TaskStateCode Normalize(string? value)
        {
            return TryGetCode(value, out TaskStateCode code)
                ? code
                : TaskStateCode.New;
        }

        public static bool TryGetCode(string? value, out TaskStateCode code)
        {
            if (!string.IsNullOrWhiteSpace(value) &&
                CodesByName.TryGetValue(value.Trim(), out TaskStateCode parsed))
            {
                code = parsed;
                return true;
            }

            code = TaskStateCode.New;
            return false;
        }
    }
}
