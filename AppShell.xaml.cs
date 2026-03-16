using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using SDNet.Models;
using SDNet.Services.Theming;
using Font = Microsoft.Maui.Font;

namespace SDNet
{
    public partial class AppShell : Shell
    {
        private readonly IThemeService _themeService;
        private readonly IUserSettingsService _userSettingsService;
        private readonly CurrentUserContext _currentUserContext;
        private bool _isSyncingThemeSelector;

        public AppShell(
            IThemeService themeService,
            IUserSettingsService userSettingsService,
            CurrentUserContext currentUserContext)
        {
            _themeService = themeService;
            _userSettingsService = userSettingsService;
            _currentUserContext = currentUserContext;

            InitializeComponent();
            _themeService.ThemeChanged += OnThemeChanged;
            ApplyRoleVisibility();
            SyncThemeSelector();
        }

        public static async Task DisplaySnackbarAsync(string message)
        {
            if (OperatingSystem.IsWindows())
            {
                if (Shell.Current is Shell shell)
                {
                    await MainThread.InvokeOnMainThreadAsync(() => shell.DisplayAlert("SDNet", message, "OK"));
                }

                return;
            }

            CancellationTokenSource cancellationTokenSource = new();

            var snackbarOptions = new SnackbarOptions
            {
                BackgroundColor = Color.FromArgb("#FF3300"),
                TextColor = Colors.White,
                ActionButtonTextColor = Colors.Yellow,
                CornerRadius = new CornerRadius(0),
                Font = Font.SystemFontOfSize(18),
                ActionButtonFont = Font.SystemFontOfSize(14)
            };

            var snackbar = Snackbar.Make(message, visualOptions: snackbarOptions);
            await snackbar.Show(cancellationTokenSource.Token);
        }

        public static async Task DisplayToastAsync(string message)
        {
            if (OperatingSystem.IsWindows())
            {
                return;
            }

            var toast = Toast.Make(message, textSize: 18);
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await toast.Show(cts.Token);
        }

        private void SfSegmentedControl_SelectionChanged(object sender, Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs e)
        {
            if (_isSyncingThemeSelector || e.NewIndex < 0)
            {
                return;
            }

            AppThemePreference preference = e.NewIndex == 0 ? AppThemePreference.Light : AppThemePreference.Dark;
            _themeService.ApplyTheme(preference);
            PersistThemePreferenceAsync(preference).FireAndForgetSafeAsync();
        }

        private async Task PersistThemePreferenceAsync(AppThemePreference preference)
        {
            var settings = await _userSettingsService.LoadAsync();
            settings.Theme = preference;
            await _userSettingsService.SaveAsync(settings);
        }

        private void SyncThemeSelector()
        {
            _isSyncingThemeSelector = true;
            ThemeSegmentedControl.SelectedIndex = _themeService.CurrentTheme == AppThemePreference.Light ? 0 : 1;
            _isSyncingThemeSelector = false;
        }

        private void OnThemeChanged(object? sender, AppThemePreference e)
        {
            MainThread.BeginInvokeOnMainThread(SyncThemeSelector);
        }

        private void ApplyRoleVisibility()
        {
            var user = _currentUserContext.CurrentUser;
            bool isAdmin = user is not null &&
                           (user.UserRoleId == 1 ||
                            string.Equals(user.UserRoleName, "Administrator", StringComparison.OrdinalIgnoreCase));

            AdminUsersShellContent.IsVisible = isAdmin;
            AdminReferencesShellContent.IsVisible = isAdmin;
            AdminTaskHistoryShellContent.IsVisible = isAdmin;
        }
    }
}
