namespace SDNet.Models
{
    public static class SDTaskTypes
    {
        public const string ITTask = "IT задача";
        public const string HardwareTask = "Оборудование";
        public const string CommunicationTask = "Коммуникации";
        public const string AccessTask = "Доступ";
        public const string SecurityTask = "Информационная безопасность";
        public const string IntegrationTask = "Интеграции";

        public static IReadOnlyList<string> All { get; } =
        [
            ITTask,
            HardwareTask,
            CommunicationTask,
            AccessTask,
            SecurityTask,
            IntegrationTask
        ];
    }
}
