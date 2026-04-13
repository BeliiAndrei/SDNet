using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SDNet.Models;
using SDNet.Services;
using SDNet.Services.TaskEvents;

namespace SDNet.PageModels
{
    public partial class MyAssignmentsPageModel : ObservableObject
    {
        private readonly CurrentUserContext _currentUserContext;
        private readonly ITaskBoardReadService _taskBoardReadService;

        public ObservableCollection<SDTask> AssignedTasks { get; } = [];

        [ObservableProperty]
        private string _summary = string.Empty;

        public MyAssignmentsPageModel(
            CurrentUserContext currentUserContext,
            ITaskBoardReadService taskBoardReadService)
        {
            _currentUserContext = currentUserContext;
            _taskBoardReadService = taskBoardReadService;
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
            AssignedTasks.Clear();

            string performerName = _currentUserContext.CurrentUser?.UserFullName ?? string.Empty;
            IReadOnlyList<SDTask> tasks = string.IsNullOrWhiteSpace(performerName)
                ? []
                : _taskBoardReadService.GetAssignedTo(performerName);

            foreach (SDTask task in tasks.OrderBy(task => task.DateNeedClose))
            {
                AssignedTasks.Add(task);
            }

            Summary = tasks.Count == 0
                ? "У вас нет назначенных задач."
                : $"Назначено задач: {tasks.Count}";
        }
    }
}
