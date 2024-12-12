using MicaForEveryone.CoreUI;
using System;
using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;
using Windows.UI.ViewManagement;

namespace MicaForEveryone.App.Services;

internal class ThemingService : IThemingService
{
    public event EventHandler? ThemeChanged;
    private UISettings uiSettings;

    public ThemingService()
    {
        uiSettings = new UISettings();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        uiSettings.ColorValuesChanged += (_, _) => ThemeChanged?.Invoke(this, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    public bool IsDarkMode()
    {
        var background = uiSettings.GetColorValue(UIColorType.Background);
        return ((5 * background.G) + (2 * background.R) + background.B) <= (8 * 128);
    }
}
