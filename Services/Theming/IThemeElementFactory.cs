using SDNet.Models;

namespace SDNet.Services.Theming
{
    public interface IThemeElementFactory
    {
        AppThemePreference ThemePreference { get; }

        AppTheme MauiTheme { get; }

        IPageThemeElement CreatePageElement();

        ICardThemeElement CreateCardElement();

        IToggleThemeElement CreateToggleElement();
    }
}
