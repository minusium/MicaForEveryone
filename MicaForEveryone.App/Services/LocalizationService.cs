using MicaForEveryone.CoreUI;
using MicaForEveryone.Models;
using Microsoft.Windows.ApplicationModel.Resources;

namespace MicaForEveryone.App.Services;

// TODO: actually implement this class
public sealed class LocalizationService : ILocalizationService
{
    public string GetLocalizedBackdropType(BackdropType backdropType)
    {
        return $"_{backdropType}";
    }

    public string GetLocalizedCornerPreference(CornerPreference cornerPreference)
    {
        return $"_{cornerPreference}";
    }

    public string GetLocalizedString(string key)
    {
        throw new System.NotImplementedException();
    }

    public string GetLocalizedTitleBarColor(TitleBarColorMode titleBarColorMode)
    {
        return $"_{titleBarColorMode}";
    }
}