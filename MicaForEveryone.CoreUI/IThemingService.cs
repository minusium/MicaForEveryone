namespace MicaForEveryone.CoreUI;

public interface IThemingService
{
    bool IsDarkMode();

    event EventHandler? ThemeChanged;
}