﻿using MicaForEveryone.Models;
using System;

namespace MicaForEveryone.App.Helpers;

public static class EnumHelper
{
    private static TitleBarColorMode[]? _titleBarColorModes;

    public static TitleBarColorMode[] TitleBarColorModes
    {
        get
        {
            return _titleBarColorModes ??= Enum.GetValues<TitleBarColorMode>();
        }
    }

    private static CornerPreference[]? _cornerPreferences;

    public static CornerPreference[] CornerPreferences
    {
        get
        {
            return _cornerPreferences ??= Enum.GetValues<CornerPreference>();
        }
    }
}