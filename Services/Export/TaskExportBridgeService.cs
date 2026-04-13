using SDNet.Models;

namespace SDNet.Services.Export
{
    public interface ITaskExportService
    {
        Task<string> ExportSingleTaskAsync(
            ExportFormat format,
            SDTask task,
            UserInfo? currentUser,
            CancellationToken cancellationToken = default);

        Task<string> ExportTaskListAsync(
            ExportFormat format,
            IReadOnlyList<SDTask> tasks,
            UserInfo? currentUser,
            CancellationToken cancellationToken = default);
    }

    public sealed class TaskExportBridgeService : ITaskExportService
    {
        public Task<string> ExportSingleTaskAsync(
            ExportFormat format,
            SDTask task,
            UserInfo? currentUser,
            CancellationToken cancellationToken = default)
        {
            var data = new TaskExportData
            {
                SelectedTask = task,
                Tasks = [task],
                GeneratedAt = DateTime.Now,
                GeneratedBy = currentUser?.UserFullName ?? currentUser?.UserName ?? "SDNet"
            };

            TaskExportDocument document = new SingleTaskDocument(CreateRenderer(format));
            return document.ExportAsync(data, GetOutputDirectory(), cancellationToken);
        }

        public Task<string> ExportTaskListAsync(
            ExportFormat format,
            IReadOnlyList<SDTask> tasks,
            UserInfo? currentUser,
            CancellationToken cancellationToken = default)
        {
            var data = new TaskExportData
            {
                Tasks = tasks,
                GeneratedAt = DateTime.Now,
                GeneratedBy = currentUser?.UserFullName ?? currentUser?.UserName ?? "SDNet"
            };

            TaskExportDocument document = new TaskListDocument(CreateRenderer(format));
            return document.ExportAsync(data, GetOutputDirectory(), cancellationToken);
        }

        private static IExportRenderer CreateRenderer(ExportFormat format)
        {
            return format switch
            {
                ExportFormat.Word => new WordExportRenderer(),
                ExportFormat.Excel => new ExcelExportRenderer(),
                ExportFormat.Pdf => new PdfExportRenderer(),
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Неизвестный формат экспорта.")
            };
        }

        private static string GetOutputDirectory()
        {
            string root = GetExportRootDirectory();

            string outputDirectory = Path.Combine(root, "SDNet", "Exports");
            Directory.CreateDirectory(outputDirectory);
            return outputDirectory;
        }

        private static string GetExportRootDirectory()
        {
#if ANDROID
            string? androidExternalDirectory =
                Android.App.Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments)?.AbsolutePath;

            if (!string.IsNullOrWhiteSpace(androidExternalDirectory))
            {
                return androidExternalDirectory;
            }
#endif

            string documentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!string.IsNullOrWhiteSpace(documentsDirectory))
            {
                return documentsDirectory;
            }

            return FileSystem.Current.AppDataDirectory;
        }
    }
}
