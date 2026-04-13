using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SDNet.Models;
using SDNet.Services.TaskEvents;
using SDNet.Services.TaskStatusAudit;

namespace SDNet.PageModels
{
    public partial class OperationsDashboardPageModel : ObservableObject
    {
        private readonly ITaskBoardReadService _taskBoardReadService;
        private readonly ITaskStatusChangeHistoryService _taskStatusChangeHistoryService;

        public ObservableCollection<TaskStatusChangeHistoryItem> RecentChanges { get; } = [];

        [ObservableProperty]
        private int _totalTasks;

        [ObservableProperty]
        private int _overdueTasks;

        [ObservableProperty]
        private int _closedTasks;

        [ObservableProperty]
        private int _tasksInApproval;

        public OperationsDashboardPageModel(
            ITaskBoardReadService taskBoardReadService,
            ITaskStatusChangeHistoryService taskStatusChangeHistoryService)
        {
            _taskBoardReadService = taskBoardReadService;
            _taskStatusChangeHistoryService = taskStatusChangeHistoryService;
        }

        [RelayCommand]
        private void Appearing()
        {
            Load();
        }

        [RelayCommand]
        private void Reload()
        {
            Load();
        }

        private void Load()
        {
            IReadOnlyList<SDTask> tasks = _taskBoardReadService.GetAll();
            TotalTasks = tasks.Count;
            OverdueTasks = tasks.Count(task => task.DateClosed is null && task.DateNeedClose.Date < DateTime.Today);
            ClosedTasks = tasks.Count(task => task.StateId == (int)TaskStateCode.Closed);
            TasksInApproval = tasks.Count(task => task.StateId == (int)TaskStateCode.Approval);

            RecentChanges.Clear();
            foreach (TaskStatusChangeHistoryItem item in _taskStatusChangeHistoryService.GetHistory().Take(8))
            {
                RecentChanges.Add(item);
            }
        }
    }
}
