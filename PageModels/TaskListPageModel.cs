using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SDNet.Data;
using SDNet.Models;
using SDNet.Services;
using SDNet.Services.Export;

namespace SDNet.PageModels
{
    public partial class TaskListPageModel : ObservableObject, IQueryAttributable
    {
        private const string AllOption = "Все";

        private readonly ISDTaskStore _taskStore;
        private readonly CurrentUserContext _currentUserContext;
        private readonly ITaskReferenceDataService _taskReferenceDataService;
        private readonly ITaskExportService _taskExportService;
        private Guid? _pendingFocusId;

        public ObservableCollection<string> Departments { get; } = [];
        public ObservableCollection<string> QueryTypes { get; } = [];

        public double TableWidth => 2800;

        [ObservableProperty]
        private ObservableCollection<SDTask> _filteredTasks = [];

        [ObservableProperty]
        private SDTask? _selectedTask;

        [ObservableProperty]
        private Guid? _focusTaskId;

        [ObservableProperty]
        private string _queryIdFilter = string.Empty;

        [ObservableProperty]
        private string _selectedDepartment = AllOption;

        [ObservableProperty]
        private string _selectedQueryType = AllOption;

        [ObservableProperty]
        private DateTime _dateFrom = DateTime.Today.AddDays(-30);

        [ObservableProperty]
        private DateTime _dateTo = DateTime.Today.AddDays(30);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FooterSummary))]
        private int _totalCount;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FooterSummary))]
        private int _inWorkCount;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FooterSummary))]
        private int _overdueCount;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FooterSummary))]
        private int _closedCount;

        [ObservableProperty]
        private DateTime _lastUpdatedAt = DateTime.Now;

        public string FooterSummary =>
            $"Всего: {TotalCount} | В работе: {InWorkCount} | Просрочено: {OverdueCount} | Закрыто: {ClosedCount}";

        public TaskListPageModel(
            ISDTaskStore taskStore,
            CurrentUserContext currentUserContext,
            ITaskReferenceDataService taskReferenceDataService,
            ITaskExportService taskExportService)
        {
            _taskStore = taskStore;
            _currentUserContext = currentUserContext;
            _taskReferenceDataService = taskReferenceDataService;
            _taskExportService = taskExportService;

            LoadFilterOptions();
            ApplyFiltersCore();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("focusId", out var focusObj) &&
                Guid.TryParse(focusObj?.ToString(), out var focusId))
            {
                _pendingFocusId = focusId;
            }

            if (query.ContainsKey("refresh"))
            {
                ApplyFiltersCore();
            }
        }

        [RelayCommand]
        private void Appearing()
        {
            LoadFilterOptions();
            ApplyFiltersCore();
            TryFocusPendingTask();
        }

        [RelayCommand]
        private void ApplyFilters()
        {
            ApplyFiltersCore();
            TryFocusPendingTask();
        }

        [RelayCommand]
        private void ResetFilters()
        {
            QueryIdFilter = string.Empty;
            SelectedDepartment = AllOption;
            SelectedQueryType = AllOption;
            DateFrom = DateTime.Today.AddDays(-30);
            DateTo = DateTime.Today.AddDays(30);

            ApplyFiltersCore();
            TryFocusPendingTask();
        }

        [RelayCommand]
        private Task CreateTask()
        {
            return Shell.Current.GoToAsync("sdtask-edit?isNew=true");
        }

        [RelayCommand]
        private async Task CloneTask(SDTask task)
        {
            SDTask clone = _taskStore.Clone(task.Id);
            ApplyFiltersCore();
            FocusOnTask(clone.Id);
            await AppShell.DisplaySnackbarAsync($"Скопирована задача №{clone.UserQueryId}");
        }

        [RelayCommand]
        private Task EditTask(SDTask task)
        {
            return Shell.Current.GoToAsync($"sdtask-edit?id={task.Id}");
        }

        [RelayCommand]
        private Task ExportSelectedTaskWord() => ExportSelectedTaskAsync(ExportFormat.Word);

        [RelayCommand]
        private Task ExportSelectedTaskExcel() => ExportSelectedTaskAsync(ExportFormat.Excel);

        [RelayCommand]
        private Task ExportSelectedTaskPdf() => ExportSelectedTaskAsync(ExportFormat.Pdf);

        [RelayCommand]
        private Task ExportTaskListWord() => ExportTaskListAsync(ExportFormat.Word);

        [RelayCommand]
        private Task ExportTaskListExcel() => ExportTaskListAsync(ExportFormat.Excel);

        [RelayCommand]
        private Task ExportTaskListPdf() => ExportTaskListAsync(ExportFormat.Pdf);

        private void ApplyFiltersCore()
        {
            IEnumerable<SDTask> query = _taskStore.GetAll();
            UserInfo? currentUser = _currentUserContext.CurrentUser;
            if (currentUser is not null && !IsAdministrator(currentUser))
            {
                query = query.Where(t =>
                    string.Equals(t.UserDepartName, currentUser.UserDepartName, StringComparison.OrdinalIgnoreCase));
            }

            if (int.TryParse(QueryIdFilter, out int taskId))
            {
                query = query.Where(t => t.UserQueryId == taskId);
            }

            if (!string.IsNullOrWhiteSpace(SelectedDepartment) && SelectedDepartment != AllOption)
            {
                query = query.Where(t =>
                    string.Equals(t.UserDepartName, SelectedDepartment, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(t.PerformerDepartName, SelectedDepartment, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(SelectedQueryType) && SelectedQueryType != AllOption)
            {
                query = query.Where(t =>
                    string.Equals(t.QueryTypeName, SelectedQueryType, StringComparison.OrdinalIgnoreCase));
            }

            DateTime start = DateFrom.Date;
            DateTime end = DateTo.Date;
            if (start <= end)
            {
                query = query.Where(t => t.DateReg.Date >= start && t.DateReg.Date <= end);
            }

            List<SDTask> result = query.ToList();
            FilteredTasks = new ObservableCollection<SDTask>(result);
            UpdateFooter(result);
            LastUpdatedAt = DateTime.Now;
        }

        private void LoadFilterOptions()
        {
            string previousDepartment = SelectedDepartment;
            string previousQueryType = SelectedQueryType;

            Departments.Clear();
            Departments.Add(AllOption);
            foreach (string department in _taskReferenceDataService.GetDepartments())
            {
                if (string.IsNullOrWhiteSpace(department))
                {
                    continue;
                }

                if (!Departments.Any(d => string.Equals(d, department, StringComparison.OrdinalIgnoreCase)))
                {
                    Departments.Add(department);
                }
            }

            QueryTypes.Clear();
            QueryTypes.Add(AllOption);
            foreach (string queryType in _taskReferenceDataService.GetQueryTypes())
            {
                if (string.IsNullOrWhiteSpace(queryType))
                {
                    continue;
                }

                if (!QueryTypes.Any(q => string.Equals(q, queryType, StringComparison.OrdinalIgnoreCase)))
                {
                    QueryTypes.Add(queryType);
                }
            }

            SelectedDepartment = Departments.Any(d => string.Equals(d, previousDepartment, StringComparison.OrdinalIgnoreCase))
                ? previousDepartment
                : AllOption;

            SelectedQueryType = QueryTypes.Any(q => string.Equals(q, previousQueryType, StringComparison.OrdinalIgnoreCase))
                ? previousQueryType
                : AllOption;
        }

        private void TryFocusPendingTask()
        {
            if (_pendingFocusId.HasValue)
            {
                FocusOnTask(_pendingFocusId.Value);
                _pendingFocusId = null;
            }
        }

        private void FocusOnTask(Guid taskId)
        {
            SDTask? task = FilteredTasks.FirstOrDefault(t => t.Id == taskId);
            if (task is null)
            {
                return;
            }

            SelectedTask = task;
            FocusTaskId = null;
            FocusTaskId = taskId;
        }

        private void UpdateFooter(IReadOnlyCollection<SDTask> tasks)
        {
            DateTime today = DateTime.Today;

            TotalCount = tasks.Count;
            ClosedCount = tasks.Count(t => t.DateClosed.HasValue);
            InWorkCount = tasks.Count(t => !t.DateClosed.HasValue);
            OverdueCount = tasks.Count(t => !t.DateClosed.HasValue && t.DateNeedClose.Date < today);
        }

        private async Task ExportSelectedTaskAsync(ExportFormat format)
        {
            if (SelectedTask is null)
            {
                await AppShell.DisplaySnackbarAsync("Сначала выберите задачу для экспорта.");
                return;
            }

            string outputPath = await _taskExportService.ExportSingleTaskAsync(
                format,
                SelectedTask,
                _currentUserContext.CurrentUser);

            await AppShell.DisplaySnackbarAsync($"Файл сохранен: {outputPath}");
        }

        private async Task ExportTaskListAsync(ExportFormat format)
        {
            List<SDTask> tasks = FilteredTasks.ToList();
            if (tasks.Count == 0)
            {
                await AppShell.DisplaySnackbarAsync("Нет задач для экспорта по текущим фильтрам.");
                return;
            }

            string outputPath = await _taskExportService.ExportTaskListAsync(
                format,
                tasks,
                _currentUserContext.CurrentUser);

            await AppShell.DisplaySnackbarAsync($"Файл сохранен: {outputPath}");
        }

        private static bool IsAdministrator(UserInfo user)
        {
            return user.UserRoleId == 1
                || string.Equals(user.UserRoleName, "Administrator", StringComparison.OrdinalIgnoreCase);
        }
    }
}
