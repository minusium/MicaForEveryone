using MicaForEveryone.App.ViewModels;
using MicaForEveryone.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MicaForEveryone.App.Views;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SettingsWindow : Window
{
    private SettingsViewModel ViewModel { get; }

    private MenuFlyout _addNewItemFlyout = new();

    public SettingsWindow()
    {
        this.InitializeComponent();

        MenuFlyoutItem processRuleItem = new() { Text = "_Add Process Rule" };
        processRuleItem.Click += (_, _) =>
        {
            AddProcessRuleContentDialog addProcessRuleContentDialog = new();
            addProcessRuleContentDialog.XamlRoot = Content.XamlRoot;
            _ = addProcessRuleContentDialog.ShowAsync();
        };
        _addNewItemFlyout.Items.Add(processRuleItem);

        MenuFlyoutItem classRuleItem = new() { Text = "_Add Class Rule" };
        classRuleItem.Click += (_, _) =>
        {
            AddClassRuleContentDialog addClassRuleContentDialog = new();
            addClassRuleContentDialog.XamlRoot = Content.XamlRoot;
            _ = addClassRuleContentDialog.ShowAsync();
        };
        _addNewItemFlyout.Items.Add(classRuleItem);

        ViewModel = App.Services.GetRequiredService<SettingsViewModel>();

        ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.ButtonBackgroundColor = AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        ChangeButtonBackground();
        Title = "_Mica For Everyone Settings";
        AppWindow.SetIcon("Assets\\MicaForEveryone.ico");
    }

    private unsafe void Window_Closed(object sender, WindowEventArgs args)
    {
        args.Handled = true;
        AppWindow.Hide();
    }

    private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer.Tag is SettingsNavigationItem { Tag: "AddRuleNavViewItem" })
        {
            _addNewItemFlyout.ShowAt(args.InvokedItemContainer);
        }
    }

    private void NavigationViewControl_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is Rule rule)
        {
            _contentFrame.Navigate(typeof(RuleSettingsPage), rule);
        }
        else
        {
            _contentFrame.Navigate(typeof(AppSettingsPage));
        }
    }

    private void Window_Activated(object sender, WindowActivatedEventArgs args)
    {
        if (args.WindowActivationState == WindowActivationState.Deactivated)
        {
            VisualStateManager.GoToState(RootPage, "TitleBarInactivated", false);
        }
        else
        {
            VisualStateManager.GoToState(RootPage, "TitleBarActive", false);
        }

    }

    private void ChangeButtonBackground()
    {
        AppWindow.TitleBar.ButtonHoverBackgroundColor = (Color)Application.Current.Resources["SubtleFillColorSecondary"];
        AppWindow.TitleBar.ButtonPressedBackgroundColor = (Color)Application.Current.Resources["SubtleFillColorTertiary"];
        AppWindow.TitleBar.ButtonForegroundColor = AppWindow.TitleBar.ButtonHoverForegroundColor = (Color)Application.Current.Resources["TextFillColorPrimary"];
    }

    private void RootPage_ActualThemeChanged(FrameworkElement sender, object args) => ChangeButtonBackground();
}
public partial class SettingsNavigationItem
{
    public string? Uid { get; set; }

    public string? Tag { get; set; }

    public IconElement? Icon { get; set; }

    public bool Selectable { get; set; } = true;
}