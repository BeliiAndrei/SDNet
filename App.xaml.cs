using SDNet.Models;
using SDNet.Services.Navigation;
using SDNet.Services.Theming;

namespace SDNet
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUserSettingsService _userSettingsService;
        private readonly IThemeService _themeService;
        private readonly IAppNavigationService _appNavigationService;
        private Window? _window;

        public App(
            IServiceProvider serviceProvider,
            IUserSettingsService userSettingsService,
            IThemeService themeService,
            IAppNavigationService appNavigationService)
        {
            _serviceProvider = serviceProvider;
            _userSettingsService = userSettingsService;
            _themeService = themeService;
            _appNavigationService = appNavigationService;

            InitializeComponent();
            _appNavigationService.OpenShellRequested += OnOpenShellRequested;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var loginPage = _serviceProvider.GetRequiredService<LoginPage>();
            InitializeUserSettingsAsync().FireAndForgetSafeAsync();

            _window = new Window(new NavigationPage(loginPage));
            return _window;
        }

        private async Task InitializeUserSettingsAsync()
        {
            UserSettings settings = await _userSettingsService.LoadAsync();
            _themeService.ApplyTheme(settings.Theme);
        }

        private void OnOpenShellRequested(object? sender, EventArgs e)
        {
            if (_window is null)
            {
                return;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                var appShell = _serviceProvider.GetRequiredService<AppShell>();
                _window.Page = appShell;
            });
        }
    }
}
