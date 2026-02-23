using SDNet.Models;

namespace SDNet.Services.Theming
{
    public sealed class DarkThemeElementFactory : IThemeElementFactory
    {
        public AppThemePreference ThemePreference => AppThemePreference.Dark;

        public AppTheme MauiTheme => AppTheme.Dark;

        public IPageThemeElement CreatePageElement()
        {
            return new PageThemeElement(
                Color.FromArgb("#17171A"),
                Color.FromArgb("#222228"),
                Color.FromArgb("#F3F3F3"),
                Color.FromArgb("#C3C3C3"));
        }

        public ICardThemeElement CreateCardElement()
        {
            return new CardThemeElement(
                Color.FromArgb("#2A2A32"),
                Color.FromArgb("#404040"));
        }

        public IToggleThemeElement CreateToggleElement()
        {
            return new ToggleThemeElement(
                Color.FromArgb("#9880E5"),
                Color.FromArgb("#404040"));
        }
    }
}
