using SDNet.Models;

namespace SDNet.Services.Notifications
{
    public interface INotificationGateway
    {
        void Send(NotificationMessage message);
    }

    public sealed class MockEmailNotificationGateway : INotificationGateway
    {
        private readonly IUserSettingsService _userSettingsService;

        public MockEmailNotificationGateway(IUserSettingsService userSettingsService)
        {
            _userSettingsService = userSettingsService;
        }

        public void Send(NotificationMessage message)
        {
            if (!_userSettingsService.Current.EnableNotifications)
            {
                return;
            }

            string recipient = string.IsNullOrWhiteSpace(message.RecipientName)
                ? message.RecipientEmail
                : message.RecipientName;

            AppShell.DisplaySnackbarAsync($"Отправлено сообщение: {message.Subject} -> {recipient}").FireAndForgetSafeAsync();
        }
    }
}
