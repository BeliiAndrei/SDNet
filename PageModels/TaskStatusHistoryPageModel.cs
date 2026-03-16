using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SDNet.Models;
using SDNet.Services;
using SDNet.Services.TaskStatusAudit;

namespace SDNet.PageModels
{
    public partial class TaskStatusHistoryPageModel : ObservableObject
    {
        private readonly CurrentUserContext _currentUserContext;
        private readonly ITaskStatusChangeHistoryService _taskStatusChangeHistoryService;

        public ObservableCollection<TaskStatusChangeHistoryItem> HistoryItems { get; } = [];

        [ObservableProperty]
        private string _taskCodeFilter = string.Empty;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        public TaskStatusHistoryPageModel(
            CurrentUserContext currentUserContext,
            ITaskStatusChangeHistoryService taskStatusChangeHistoryService)
        {
            _currentUserContext = currentUserContext;
            _taskStatusChangeHistoryService = taskStatusChangeHistoryService;
        }

        [RelayCommand]
        private async Task Appearing()
        {
            if (!IsAdministrator(_currentUserContext.CurrentUser))
            {
                await AppShell.DisplaySnackbarAsync("Доступ к истории изменения статусов есть только у администратора.");
                await Shell.Current.GoToAsync("//task-list");
                return;
            }

            LoadHistory();
        }

        [RelayCommand]
        private void ApplyFilter()
        {
            LoadHistory();
        }

        [RelayCommand]
        private void ResetFilter()
        {
            TaskCodeFilter = string.Empty;
            LoadHistory();
        }

        private void LoadHistory()
        {
            HistoryItems.Clear();
            StatusMessage = string.Empty;

            int? userQueryId = null;
            if (!string.IsNullOrWhiteSpace(TaskCodeFilter))
            {
                if (!int.TryParse(TaskCodeFilter.Trim(), out int parsedUserQueryId) || parsedUserQueryId <= 0)
                {
                    StatusMessage = "Введите корректный номер задачи.";
                    return;
                }

                userQueryId = parsedUserQueryId;
            }

            try
            {
                IReadOnlyList<TaskStatusChangeHistoryItem> items = _taskStatusChangeHistoryService.GetHistory(userQueryId);
                foreach (TaskStatusChangeHistoryItem item in items)
                {
                    HistoryItems.Add(item);
                }

                if (HistoryItems.Count == 0)
                {
                    StatusMessage = userQueryId.HasValue
                        ? "История по выбранной задаче не найдена."
                        : "История изменений пока пуста.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private static bool IsAdministrator(UserInfo? user)
        {
            return user is not null &&
                   (user.UserRoleId == 1 ||
                    string.Equals(user.UserRoleName, "Administrator", StringComparison.OrdinalIgnoreCase));
        }
    }
}
