namespace SDNet.Models
{
    public enum AppThemePreference
    {
        Light = 0,
        Dark = 1
    }

    public sealed class UserSettings
    {
        public bool SaveTableLayout { get; set; } = true;

        public AppThemePreference Theme { get; set; } = AppThemePreference.Light;

        public bool CompactTaskRows { get; set; }

        public bool ConfirmBeforeDelete { get; set; } = true;

        public bool EnableNotifications { get; set; } = true;
    }
}
