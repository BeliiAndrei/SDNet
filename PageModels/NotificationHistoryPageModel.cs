using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SDNet.Models;
using SDNet.Services;
using SDNet.Services.Notifications;

namespace SDNet.PageModels
{
    public partial class NotificationHistoryPageModel : ObservableObject
    {
        private readonly CurrentUserContext _currentUserContext;
        private readonly INotificationHistoryService _notificationHistoryService;

        public ObservableCollection<NotificationMessage> Notifications { get; } = [];

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        public NotificationHistoryPageModel(
            CurrentUserContext currentUserContext,
            INotificationHistoryService notificationHistoryService)
        {
            _currentUserContext = currentUserContext;
            _notificationHistoryService = notificationHistoryService;
        }

        [RelayCommand]
        private async Task Appearing()
        {
            if (!IsAdministrator(_currentUserContext.CurrentUser))
            {
                await AppShell.DisplaySnackbarAsync("История уведомлений доступна только администратору.");
                await Shell.Current.GoToAsync("//task-list");
                return;
            }

            Load();
        }

        [RelayCommand]
        private void Reload()
        {
            Load();
        }

        private void Load()
        {
            Notifications.Clear();
            foreach (NotificationMessage item in _notificationHistoryService.GetAll())
            {
                Notifications.Add(item);
            }

            StatusMessage = Notifications.Count == 0
                ? "История уведомлений пока пуста."
                : $"Сообщений: {Notifications.Count}";
        }

        private static bool IsAdministrator(UserInfo? user)
        {
            return user is not null &&
                   (user.UserRoleId == 1 ||
                    string.Equals(user.UserRoleName, "Administrator", StringComparison.OrdinalIgnoreCase));
        }
    }
}
