using SDNet.Models;

namespace SDNet.Services.Theming
{
    public sealed class ThemeService : IThemeService
    {
        private readonly IThemeElementFactory _lightFactory;
        private readonly IThemeElementFactory _darkFactory;

        public ThemeService()
        {
            _lightFactory = new LightThemeElementFactory();
            _darkFactory = new DarkThemeElementFactory();
            CurrentTheme = AppThemePreference.Light;
        }

        public AppThemePreference CurrentTheme { get; private set; }

        public event EventHandler<AppThemePreference>? ThemeChanged;

        public void ApplyTheme(AppThemePreference preference)
        {
            IThemeElementFactory factory = preference == AppThemePreference.Dark ? _darkFactory : _lightFactory;
            CurrentTheme = factory.ThemePreference;

            if (Application.Current is null)
            {
                return;
            }

            Application.Current.UserAppTheme = factory.MauiTheme;

            var pageElement = factory.CreatePageElement();
            var cardElement = factory.CreateCardElement();
            var toggleElement = factory.CreateToggleElement();
            var resources = Application.Current.Resources;

            resources["ThemePageBackgroundColor"] = pageElement.PageBackgroundColor;
            resources["ThemeSectionBackgroundColor"] = pageElement.SectionBackgroundColor;
            resources["ThemePrimaryTextColor"] = pageElement.PrimaryTextColor;
            resources["ThemeSecondaryTextColor"] = pageElement.SecondaryTextColor;
            resources["ThemeCardBackgroundColor"] = cardElement.CardBackgroundColor;
            resources["ThemeBorderColor"] = cardElement.BorderColor;
            resources["ThemeAccentColor"] = toggleElement.AccentColor;
            resources["ThemeSwitchTrackColor"] = toggleElement.SwitchTrackColor;

            ThemeChanged?.Invoke(this, CurrentTheme);
        }
    }
}
