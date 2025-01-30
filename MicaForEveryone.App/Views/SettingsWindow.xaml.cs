using MicaForEveryone.App.ViewModels;
using MicaForEveryone.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;
using Windows.UI;

using static TerraFX.Interop.Windows.Windows;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MicaForEveryone.App.Views;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SettingsWindow : Window
{
    private SettingsViewModel ViewModel { get; }

    public SettingsWindow()
    {
        this.InitializeComponent();

        ViewModel = App.Services.GetRequiredService<SettingsViewModel>();

        ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.ButtonBackgroundColor = AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        ChangeButtonBackground();
        Title = "_Mica For Everyone Settings";
        AppWindow.SetIcon("Assets\\MicaForEveryone.ico");
        SetTitleBar(TitleBarControl);

        unsafe
        {
            HWND hwnd = new HWND((void*)WinRT.Interop.WindowNative.GetWindowHandle(this));
            SetWindowSubclass(hwnd, &WindowProc, 0, 0);

            uint dpi = GetDpiForWindow(hwnd);
            int width = (int)(900 * dpi / 96.0f);
            int height = (int)(600 * dpi / 96.0f);

            AppWindow.Resize(new Windows.Graphics.SizeInt32(width, height));

            RECT rcWorkArea;
            SystemParametersInfo(SPI.SPI_GETWORKAREA, 0, &rcWorkArea, 0);
            int x = (int)((rcWorkArea.right + rcWorkArea.left) / 2.0f - width / 2.0f);
            int y = (int)((rcWorkArea.top + rcWorkArea.bottom) / 2.0f - height / 2.0f);

            AppWindow.Move(new Windows.Graphics.PointInt32(x, y));
        }
        // NavigationViewControl.SelectedItem = NavigationViewControl.FooterMenuItems.Last();
    }

    [UnmanagedCallersOnly]
    private static unsafe LRESULT WindowProc(HWND hWND, uint arg2, WPARAM wPARAM, LPARAM lPARAM, nuint arg5, nuint arg6)
    {
        if (arg2 == WM.WM_DESTROY)
        {
            LRESULT result = DefSubclassProc(hWND, arg2, wPARAM, lPARAM);
            MSG msg;
            while (PeekMessage(&msg, HWND.NULL, 0, 0, PM.PM_REMOVE))
            {
                if (msg.message != WM.WM_QUIT)
                {
                    TranslateMessage(&msg);
                    DispatchMessage(&msg);
                    continue;
                }
                break;
            }
            return result;
        }
        if (arg2 == WM.WM_GETMINMAXINFO)
        {
            var dpi = GetDpiForWindow(hWND);
            float scale = dpi / 96.0f;
            MINMAXINFO* minMaxInfo = (MINMAXINFO*)lPARAM;
            minMaxInfo->ptMinTrackSize.x = (int)(500 * scale);
            minMaxInfo->ptMinTrackSize.y = (int)(500 * scale);
        }
        return DefSubclassProc(hWND, arg2, wPARAM, lPARAM);
    }

    private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
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
            try
            {
                // This may cause an exception on closing, not sure why.
                VisualStateManager.GoToState(RootPage, "TitleBarInactivated", false);
            }
            catch { }
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

    private void RootPage_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width < 700)
        {
            AppWindow.TitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;
        }
        else
        {
            AppWindow.TitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Standard;
        }
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        Activated -= Window_Activated;
    }
}

public sealed class AddNewRuleMenuItem;

public sealed class AppSettingsMenuItem;

public sealed partial class SettingsNavigationItemSelector : DataTemplateSelector
{
    public DataTemplate GlobalRuleTemplate { get; set; } = new();

    public DataTemplate ProcessRuleTemplate { get; set; } = new();

    public DataTemplate ClassRuleTemplate { get; set; } = new();

    public DataTemplate AddNewRuleTemplate { get; set; } = new();

    public DataTemplate AppSettingsTemplate { get; set; } = new();

    protected override DataTemplate SelectTemplateCore(object item)
    {
        if (item is GlobalRule)
            return GlobalRuleTemplate;

        if (item is ProcessRule)
            return ProcessRuleTemplate;

        if (item is ClassRule)
            return ClassRuleTemplate;

        if (item is AddNewRuleMenuItem)
            return AddNewRuleTemplate;

        if (item is AppSettingsMenuItem)
            return AppSettingsTemplate;

        throw new ArgumentException("Navigation menu item type is invalid.");
    }
}