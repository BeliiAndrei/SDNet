using SDNet.Models;

namespace SDNet.Services.Theming
{
    public sealed class LightThemeElementFactory : IThemeElementFactory
    {
        public AppThemePreference ThemePreference => AppThemePreference.Light;

        public AppTheme MauiTheme => AppTheme.Light;

        public IPageThemeElement CreatePageElement()
        {
            return new PageThemeElement(
                Color.FromArgb("#F2F2F2"),
                Color.FromArgb("#E0E0E0"),
                Color.FromArgb("#0D0D0D"),
                Color.FromArgb("#404040"));
        }

        public ICardThemeElement CreateCardElement()
        {
            return new CardThemeElement(
                Color.FromArgb("#FFFFFF"),
                Color.FromArgb("#C8C8C8"));
        }

        public IToggleThemeElement CreateToggleElement()
        {
            return new ToggleThemeElement(
                Color.FromArgb("#512BD4"),
                Color.FromArgb("#DFD8F7"));
        }
    }
}
