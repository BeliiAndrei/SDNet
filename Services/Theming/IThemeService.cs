using SDNet.Models;

namespace SDNet.Services.Theming
{
    public interface IThemeService
    {
        AppThemePreference CurrentTheme { get; }

        event EventHandler<AppThemePreference>? ThemeChanged;

        void ApplyTheme(AppThemePreference preference);
    }
}
