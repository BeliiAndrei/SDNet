using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SDNet.Models;
using SDNet.Services;
using SDNet.Services.Theming;

namespace SDNet.PageModels
{
    public partial class SettingsPageModel : ObservableObject
    {
        private readonly IUserSettingsService _settingsService;
        private readonly IThemeService _themeService;
        private bool _isLoading;

        [ObservableProperty]
        private bool _saveTableLayout;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ThemeCaption))]
        private bool _isDarkTheme;

        [ObservableProperty]
        private bool _compactTaskRows;

        [ObservableProperty]
        private bool _confirmBeforeDelete;

        [ObservableProperty]
        private bool _enableNotifications;

        [ObservableProperty]
        private string _settingsFilePath = string.Empty;

        public string ThemeCaption => IsDarkTheme ? "Темная" : "Светлая";

        public SettingsPageModel(IUserSettingsService settingsService, IThemeService themeService)
        {
            _settingsService = settingsService;
            _themeService = themeService;
        }

        partial void OnIsDarkThemeChanged(bool value)
        {
            if (_isLoading)
            {
                return;
            }

            _themeService.ApplyTheme(value ? AppThemePreference.Dark : AppThemePreference.Light);
        }

        [RelayCommand]
        private async Task Appearing()
        {
            _isLoading = true;
            try
            {
                UserSettings settings = await _settingsService.LoadAsync();
                SaveTableLayout = settings.SaveTableLayout;
                IsDarkTheme = settings.Theme == AppThemePreference.Dark;
                CompactTaskRows = settings.CompactTaskRows;
                ConfirmBeforeDelete = settings.ConfirmBeforeDelete;
                EnableNotifications = settings.EnableNotifications;
                SettingsFilePath = _settingsService.SettingsFilePath;
            }
            finally
            {
                _isLoading = false;
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            UserSettings settings = BuildSettings();
            await _settingsService.SaveAsync(settings);
            await AppShell.DisplaySnackbarAsync("Настройки сохранены");
        }

        [RelayCommand]
        private async Task RestoreDefaults()
        {
            _isLoading = true;
            try
            {
                SaveTableLayout = true;
                IsDarkTheme = false;
                CompactTaskRows = false;
                ConfirmBeforeDelete = true;
                EnableNotifications = true;
            }
            finally
            {
                _isLoading = false;
            }

            _themeService.ApplyTheme(AppThemePreference.Light);
            await Save();
        }

        private UserSettings BuildSettings()
        {
            return new UserSettings
            {
                SaveTableLayout = SaveTableLayout,
                Theme = IsDarkTheme ? AppThemePreference.Dark : AppThemePreference.Light,
                CompactTaskRows = CompactTaskRows,
                ConfirmBeforeDelete = ConfirmBeforeDelete,
                EnableNotifications = EnableNotifications
            };
        }
    }
}
