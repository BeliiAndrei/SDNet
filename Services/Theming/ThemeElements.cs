namespace SDNet.Services.Theming
{
    public interface IPageThemeElement
    {
        Color PageBackgroundColor { get; }

        Color SectionBackgroundColor { get; }

        Color PrimaryTextColor { get; }

        Color SecondaryTextColor { get; }
    }

    public interface ICardThemeElement
    {
        Color CardBackgroundColor { get; }

        Color BorderColor { get; }
    }

    public interface IToggleThemeElement
    {
        Color AccentColor { get; }

        Color SwitchTrackColor { get; }
    }

    public sealed record PageThemeElement(
        Color PageBackgroundColor,
        Color SectionBackgroundColor,
        Color PrimaryTextColor,
        Color SecondaryTextColor) : IPageThemeElement;

    public sealed record CardThemeElement(
        Color CardBackgroundColor,
        Color BorderColor) : ICardThemeElement;

    public sealed record ToggleThemeElement(
        Color AccentColor,
        Color SwitchTrackColor) : IToggleThemeElement;
}
