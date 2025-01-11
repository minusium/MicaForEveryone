using MicaForEveryone.CoreUI;
using MicaForEveryone.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MicaForEveryone.App.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class RuleSettingsPage : Page
{
    private Rule? Rule { get; set; }
    private ISettingsService SettingsService { get; }
    private IRuleService RuleService { get; }

    public RuleSettingsPage()
    {
        this.InitializeComponent();

        RuleService = App.Services.GetRequiredService<IRuleService>();
        SettingsService = App.Services.GetRequiredService<ISettingsService>();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        Rule = Unsafe.As<Rule>(e.Parameter);
        Rule.PropertyChanged += Rule_PropertyChanged;
        base.OnNavigatedTo(e);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        Rule!.PropertyChanged -= Rule_PropertyChanged;
        base.OnNavigatedFrom(e);
    }

    private void Rule_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        _ = SettingsService.SaveAsync().ConfigureAwait(false);
        _ = RuleService.ApplyRulesToAllWindowsAsync().ConfigureAwait(false);
    }

    public static string GetTitleBarColorLocalized(TitleBarColorMode titleBarColorMode)
    {
        return App.Services.GetRequiredService<ILocalizationService>().GetLocalizedTitleBarColor(titleBarColorMode);
    }

    public static string GetBackdropTypeLocalized(BackdropType backdropType)
    {
        return App.Services.GetRequiredService<ILocalizationService>().GetLocalizedBackdropType(backdropType);
    }

    public static string GetCornerPreferenceLocalized(CornerPreference cornerPreference)
    {
        return App.Services.GetRequiredService<ILocalizationService>().GetLocalizedCornerPreference(cornerPreference);
    }

    public static Visibility NotGlobalRuleVisibility(Rule rule)
    {
        return rule is GlobalRule ? Visibility.Collapsed : Visibility.Visible;
    }

    private void SettingsCard_Click(object sender, RoutedEventArgs e)
    {
        SettingsService.Settings!.Rules.Remove(Rule!);
        _ = SettingsService.SaveAsync().ConfigureAwait(false);
        _ = RuleService.ApplyRulesToAllWindowsAsync().ConfigureAwait(false);
    }
}
