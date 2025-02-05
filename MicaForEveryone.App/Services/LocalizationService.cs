using MicaForEveryone.CoreUI;
using MicaForEveryone.Models;
using Microsoft.Windows.ApplicationModel.Resources;
using System;

namespace MicaForEveryone.App.Services;

// TODO: actually implement this class
public sealed class LocalizationService : ILocalizationService
{
    private readonly ResourceLoader resourceLoader = new();

    public string GetLocalizedBackdropType(BackdropType backdropType)
    {
        return backdropType switch
        {
            BackdropType.Default => resourceLoader.GetString("DefaultBackdropName"),
            BackdropType.None => resourceLoader.GetString("NoneBackdropName"),
            BackdropType.Mica => resourceLoader.GetString("MicaBackdropName"),
            BackdropType.Acrylic => resourceLoader.GetString("AcrylicBackdropName"),
            BackdropType.MicaAlt => resourceLoader.GetString("MicaAltBackdropName"),
            _ => throw new ArgumentException("Invalid backdrop type.", nameof(backdropType)),
        };
    }

    public string GetLocalizedCornerPreference(CornerPreference cornerPreference)
    {
        return cornerPreference switch {
            CornerPreference.Default => resourceLoader.GetString("DefaultCornerPreference"),
            CornerPreference.RoundedSmall => resourceLoader.GetString("RoundedSmallCornerPreference"),
            CornerPreference.Rounded => resourceLoader.GetString("RoundedCornerPreference"),
            CornerPreference.Square => resourceLoader.GetString("SquareCornerPreference"),
            _ => throw new ArgumentException("Invalid corner preference.", nameof(cornerPreference)),
        };
    }

    public string GetLocalizedString(string key)
    {
        return resourceLoader.GetString(key);
    }

    public string GetLocalizedTitleBarColor(TitleBarColorMode titleBarColorMode)
    {
        return titleBarColorMode switch {
            TitleBarColorMode.Default => resourceLoader.GetString("DefaultTitleBarColorMode"),
            TitleBarColorMode.Light => resourceLoader.GetString("LightTitleBarColorMode"),
            TitleBarColorMode.Dark => resourceLoader.GetString("DarkTitleBarColorMode"),
            TitleBarColorMode.System => resourceLoader.GetString("SystemTitleBarColorMode"),
            TitleBarColorMode.Custom => resourceLoader.GetString("CustomTitleBarColorMode"),
            _ => throw new ArgumentException("Invalid title bar color mode.", nameof(titleBarColorMode)),
        };
    }

    public string GetRuleName(Rule rule)
    {
        if (rule is GlobalRule)
            return resourceLoader.GetString("GlobalRuleName");
        if (rule is ProcessRule processRule)
            return processRule.ProcessName;
        if (rule is ClassRule classRule)
            return classRule.ClassName;
        throw new ArgumentException("Invalid rule type.", nameof(rule));
    }
}