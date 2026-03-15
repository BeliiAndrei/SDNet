using SDNet.Models;

namespace SDNet.Services.Export
{
    public enum ExportFormat
    {
        Word = 0,
        Excel = 1,
        Pdf = 2
    }

    public sealed class TaskExportData
    {
        public SDTask? SelectedTask { get; set; }

        public IReadOnlyList<SDTask> Tasks { get; set; } = [];

        public DateTime GeneratedAt { get; set; } = DateTime.Now;

        public string GeneratedBy { get; set; } = string.Empty;
    }

    public interface IExportRenderer
    {
        string FileExtension { get; }

        void BeginDocument(string title);

        void WriteHeading(string text);

        void WriteParagraph(string text);

        void WriteField(string name, string value);

        void WriteTable(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows);

        Task<string> SaveAsync(string outputDirectory, string fileNameWithoutExtension, CancellationToken cancellationToken = default);
    }

    public abstract class TaskExportDocument
    {
        protected readonly IExportRenderer Renderer;

        protected TaskExportDocument(IExportRenderer renderer)
        {
            Renderer = renderer;
        }

        public async Task<string> ExportAsync(
            TaskExportData data,
            string outputDirectory,
            CancellationToken cancellationToken = default)
        {
            Renderer.BeginDocument(GetTitle(data));
            WriteTitle(data);
            WriteContent(data);

            return await Renderer.SaveAsync(outputDirectory, GetFileName(data), cancellationToken);
        }

        protected abstract string GetTitle(TaskExportData data);

        protected abstract string GetFileName(TaskExportData data);

        protected abstract void WriteTitle(TaskExportData data);

        protected abstract void WriteContent(TaskExportData data);

        protected static string FormatDate(DateTime value) => value.ToString("dd.MM.yyyy HH:mm");

        protected static string FormatDate(DateTime? value) => value?.ToString("dd.MM.yyyy HH:mm") ?? "—";

        protected static string ValueOrDash(string? value) => string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();

        protected void WriteBaseTaskFields(SDTask task)
        {
            Renderer.WriteHeading("Основные сведения");
            Renderer.WriteField("№ запроса", task.UserQueryId.ToString());
            Renderer.WriteField("Тип задачи", task.TaskTypeName);
            Renderer.WriteField("Дата регистрации", FormatDate(task.DateReg));
            Renderer.WriteField("Приоритет", ValueOrDash(task.Priority));
            Renderer.WriteField("Пользователь", ValueOrDash(task.UserFio));
            Renderer.WriteField("Подразделение", ValueOrDash(task.UserDepartName));
            Renderer.WriteField("Тег", ValueOrDash(task.UserQueryTag));
            Renderer.WriteField("Тип запроса", ValueOrDash(task.QueryTypeName));
            Renderer.WriteField("IT-проект", ValueOrDash(task.ItProjectName));
            Renderer.WriteField("Краткое описание", ValueOrDash(task.ShortDescription));
            Renderer.WriteField("Состояние", ValueOrDash(task.StateName));
            Renderer.WriteField("Срок закрытия", FormatDate(task.DateNeedClose));
            Renderer.WriteField("Исполнитель", ValueOrDash(task.PerformerName));
            Renderer.WriteField("Отдел исполнителя", ValueOrDash(task.PerformerDepartName));
            Renderer.WriteField("Процент выполнения", $"{task.PerformPercent}%");
            Renderer.WriteField("Дата закрытия", FormatDate(task.DateClosed));
        }

        protected void WriteTaskSpecificFields(SDTask task)
        {
            Renderer.WriteHeading("Специфика типа задачи");

            switch (task)
            {
                case ITTask itTask:
                    Renderer.WriteField("Системная область", ValueOrDash(itTask.SystemArea));
                    Renderer.WriteField("Требует деплоя", itTask.RequiresDeployment ? "Да" : "Нет");
                    break;
                case HardwareTask hardwareTask:
                    Renderer.WriteField("Модель оборудования", ValueOrDash(hardwareTask.EquipmentModel));
                    Renderer.WriteField("Инвентарный номер", ValueOrDash(hardwareTask.AssetNumber));
                    break;
                case CommunicationTask communicationTask:
                    Renderer.WriteField("Канал связи", ValueOrDash(communicationTask.Channel));
                    Renderer.WriteField("Контакт", ValueOrDash(communicationTask.ContactPoint));
                    break;
                case AccessTask accessTask:
                    Renderer.WriteField("Роль доступа", ValueOrDash(accessTask.AccessRole));
                    Renderer.WriteField("Ресурс", ValueOrDash(accessTask.ResourceName));
                    break;
                case SecurityTask securityTask:
                    Renderer.WriteField("Уровень риска", ValueOrDash(securityTask.RiskLevel));
                    Renderer.WriteField("Требует аудита", securityTask.RequiresAudit ? "Да" : "Нет");
                    break;
                case IntegrationTask integrationTask:
                    Renderer.WriteField("Endpoint", ValueOrDash(integrationTask.EndpointName));
                    Renderer.WriteField("Интегрируемая система", ValueOrDash(integrationTask.IntegrationSystem));
                    break;
                default:
                    Renderer.WriteParagraph("Дополнительных полей для данного типа задачи не найдено.");
                    break;
            }

            if (task.Notes.Count == 0)
            {
                return;
            }

            Renderer.WriteHeading("Примечания");
            foreach (string note in task.Notes)
            {
                Renderer.WriteParagraph($"• {ValueOrDash(note)}");
            }
        }
    }

    public sealed class SingleTaskDocument : TaskExportDocument
    {
        public SingleTaskDocument(IExportRenderer renderer)
            : base(renderer)
        {
        }

        protected override string GetTitle(TaskExportData data)
        {
            int taskNumber = data.SelectedTask?.UserQueryId ?? 0;
            return $"Паспорт заявки №{taskNumber}";
        }

        protected override string GetFileName(TaskExportData data)
        {
            SDTask? task = data.SelectedTask;
            int taskNumber = task?.UserQueryId ?? 0;
            return $"TaskPassport_{taskNumber}_{data.GeneratedAt:yyyyMMdd_HHmmss}";
        }

        protected override void WriteTitle(TaskExportData data)
        {
            Renderer.WriteHeading(GetTitle(data));
            Renderer.WriteParagraph($"Сформировано: {FormatDate(data.GeneratedAt)}");
            Renderer.WriteParagraph($"Автор экспорта: {ValueOrDash(data.GeneratedBy)}");
        }

        protected override void WriteContent(TaskExportData data)
        {
            SDTask? task = data.SelectedTask;
            if (task is null)
            {
                Renderer.WriteParagraph("Задача для экспорта не выбрана.");
                return;
            }

            WriteBaseTaskFields(task);
            WriteTaskSpecificFields(task);
        }
    }

    public sealed class TaskListDocument : TaskExportDocument
    {
        public TaskListDocument(IExportRenderer renderer)
            : base(renderer)
        {
        }

        protected override string GetTitle(TaskExportData data) => "Реестр задач SDNet";

        protected override string GetFileName(TaskExportData data)
        {
            return $"TaskList_{data.GeneratedAt:yyyyMMdd_HHmmss}";
        }

        protected override void WriteTitle(TaskExportData data)
        {
            Renderer.WriteHeading(GetTitle(data));
            Renderer.WriteParagraph($"Сформировано: {FormatDate(data.GeneratedAt)}");
            Renderer.WriteParagraph($"Автор экспорта: {ValueOrDash(data.GeneratedBy)}");
            Renderer.WriteParagraph($"Количество задач: {data.Tasks.Count}");
        }

        protected override void WriteContent(TaskExportData data)
        {
            if (data.Tasks.Count == 0)
            {
                Renderer.WriteParagraph("Список задач пуст. Экспортированных записей нет.");
                return;
            }

            IReadOnlyList<string> headers =
            [
                "№ запроса",
                "Дата рег.",
                "Приоритет",
                "Пользователь",
                "Тип задачи",
                "Состояние",
                "Срок",
                "Исполнитель"
            ];

            List<IReadOnlyList<string>> rows = [];
            foreach (SDTask task in data.Tasks)
            {
                rows.Add(
                [
                    task.UserQueryId.ToString(),
                    task.DateReg.ToString("dd.MM.yyyy"),
                    ValueOrDash(task.Priority),
                    ValueOrDash(task.UserFio),
                    ValueOrDash(task.TaskTypeName),
                    ValueOrDash(task.StateName),
                    task.DateNeedClose.ToString("dd.MM.yyyy"),
                    ValueOrDash(task.PerformerName)
                ]);
            }

            Renderer.WriteTable(headers, rows);
        }
    }
}
