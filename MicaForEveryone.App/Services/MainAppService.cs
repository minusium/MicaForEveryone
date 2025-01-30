using MicaForEveryone.App.Views;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using System.Runtime.InteropServices;
using System;
using Windows.Foundation;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;
using WinRT.Interop;
using WinRT;

namespace MicaForEveryone.App.Services;

public sealed unsafe class MainAppService
{
    DesktopWindowXamlSource? _source;
    HWND _mainWnd;
    SettingsWindow? _window;

    public void Initialize()
    {
        HINSTANCE instance = GetModuleHandleW(null);
        HICON largeIcon, smallIcon;
        LoadIconMetric(instance, IDI.IDI_APPLICATION, (int)_LI_METRIC.LIM_LARGE, &largeIcon);
        LoadIconMetric(instance, IDI.IDI_APPLICATION, (int)_LI_METRIC.LIM_SMALL, &smallIcon);

        fixed (char* lpClassName = "MicaForEveryoneNotificationIcon")
        {
            WNDCLASSEXW wndClass = new()
            {
                cbSize = (uint)sizeof(WNDCLASSEXW),
                style = CS.CS_HREDRAW | CS.CS_VREDRAW,
                lpfnWndProc = &WindowProc,
                hInstance = instance,
                hCursor = HCURSOR.NULL,
                lpszClassName = lpClassName,
                lpszMenuName = null,
                hIcon = largeIcon,
                hIconSm = smallIcon,
                cbClsExtra = 0,
                cbWndExtra = 0,
                hbrBackground = HBRUSH.NULL
            };

            RegisterClassExW(&wndClass);
        }
        nint gcHandlePtr = GCHandle.ToIntPtr(GCHandle.Alloc(this));

        fixed (char* lpWindowTitle = "MicaForEveryoneNotificationIcon")
        {
            _mainWnd = CreateWindowExW(WS.WS_EX_NOACTIVATE | WS.WS_EX_TOPMOST, lpWindowTitle, null, WS.WS_POPUPWINDOW, 0, 0, 0, 0, HWND.NULL, HMENU.NULL, instance, gcHandlePtr.ToPointer());
        }
        var rgn = CreateRectRgn(0, 0, 0, 0);
        SetWindowRgn(_mainWnd, rgn, false);
        // We have to show the window, or it crashes.
        ShowWindow(_mainWnd, 5);
    }

    public void ActivateSettings()
    {
        _window ??= new SettingsWindow();
        _window.Closed += _window_Closed;
        _window.Activate();
        HWND hwnd = new HWND((void*)WindowNative.GetWindowHandle(_window));
        SetForegroundWindow(hwnd);
    }

    private void _window_Closed(object sender, Microsoft.UI.Xaml.WindowEventArgs args)
    {
        ((Microsoft.UI.Xaml.Window)sender).Closed -= _window_Closed;
        _window = null;
    }

    public void Shutdown()
    {
        _window?.Close();
        DestroyWindow(_mainWnd);
    }

    [UnmanagedCallersOnly]
    private static LRESULT WindowProc(HWND hWnd, uint Msg, WPARAM wParam, LPARAM lParam)
    {
        switch (Msg)
        {
            case 1:
                {
                    HICON smallIcon;
                    LoadIconMetric(GetModuleHandleW(null), IDI.IDI_APPLICATION, (int)_LI_METRIC.LIM_SMALL, &smallIcon);

                    CREATESTRUCTW* lpCreateStruct = (CREATESTRUCTW*)&lParam;
                    nint gcHandlePtr = *(nint*)lpCreateStruct->lpCreateParams;
                    var gc = GCHandle.FromIntPtr(gcHandlePtr);
                    var appService = (MainAppService)(gc.Target!);
                    appService._source = new();
                    var thing = Win32Interop.GetWindowIdFromWindow(new IntPtr(hWnd.Value));
                    appService._source.Initialize(thing);
                    appService._source.Content = new TrayIconPage();
                    appService._source.SiteBridge.ResizePolicy = Microsoft.UI.Content.ContentSizePolicy.ResizeContentToParentWindow;
                    appService._source.SiteBridge.Show();

                    SetWindowLongPtrW(hWnd, GWL.GWL_USERDATA, gcHandlePtr);

                    NOTIFYICONDATAW notifyIconData = new();
                    notifyIconData.hWnd = hWnd;
                    notifyIconData.guidItem = new Guid([0xA0, 0x23, 0x5A, 0x9F, 0xC6, 0xB6, 0x41, 0x89, 0xAE, 0x4B, 0xAC, 0x00, 0x9F, 0xC6, 0x78, 0x7C]);
                    notifyIconData.cbSize = (uint)sizeof(NOTIFYICONDATAW);
                    notifyIconData.hIcon = smallIcon;
                    notifyIconData.uVersion = 4;
                    notifyIconData.uCallbackMessage = WM.WM_APP + 1;

                    // Currently, we can't show a tool tip for the app name,
                    // so we just tell Windows to show it for us.
                    // It might look a bit ugly, but it works.
                    notifyIconData.uFlags = NIF_ICON | NIF_MESSAGE | NIF_TIP | NIF_GUID;
                    "Mica For Everyone".CopyTo(MemoryMarshal.CreateSpan(ref notifyIconData.szTip[0], 128));
                    Shell_NotifyIconW(NIM_ADD, &notifyIconData);
                    Shell_NotifyIconW(NIM_SETVERSION, &notifyIconData);
                    break;
                }

            case WM.WM_APP + 1:
                {
                    RECT iconRect;
                    NOTIFYICONIDENTIFIER id = new NOTIFYICONIDENTIFIER()
                    {
                        cbSize = (uint)sizeof(NOTIFYICONIDENTIFIER),
                        uID = 1,
                        hWnd = hWnd
                    };
                    Shell_NotifyIconGetRect(&id, &iconRect);
                    HMONITOR monitor = MonitorFromRect(&iconRect, MONITOR.MONITOR_DEFAULTTONULL);
                    MONITORINFO monitorInfo;
                    bool monitorSuccessful = false;
                    int? workBottom = null;

                    if (monitor != HMONITOR.NULL && (monitorSuccessful = GetMonitorInfoW(monitor, &monitorInfo)))
                        if ((workBottom = monitorInfo.rcWork.bottom) < iconRect.bottom)
                            iconRect.top = workBottom.Value - 1;

                    SetWindowPos(hWnd, HWND.NULL, iconRect.left, iconRect.top, iconRect.right - iconRect.left, iconRect.bottom - iconRect.left, SWP.SWP_NOACTIVATE | SWP.SWP_NOZORDER);

                    var pointer = GetWindowLongPtrW(hWnd, GWL.GWL_USERDATA);
                    var gc = GCHandle.FromIntPtr(pointer);
                    var appService = (MainAppService)(gc.Target!);

                    switch (LOWORD(lParam))
                    {
                        case WM.WM_CONTEXTMENU:
                            var scaleFactor = GetDpiForWindow(hWnd) / 96f;
                            SetForegroundWindow(hWnd);

                            Point point = new(
                                GET_X_LPARAM(new((nint)wParam.Value)),
                                GET_Y_LPARAM(new((nint)wParam.Value))
                            );

                            point = new(
                                (point.X - iconRect.left) / scaleFactor,
                                (point.Y - iconRect.top) / scaleFactor
                            );

                            var page = (TrayIconPage)(appService._source!.Content);
                            page.ContextFlyout.As<MenuFlyout>().ShowAt(page, point);
                            break;

                        case NIN_SELECT:
                        case NIN_KEYSELECT:
                            appService.ActivateSettings();
                            break;

                    }
                    break;
                }

            case WM.WM_DESTROY:
                {
                    var pointer = GetWindowLongPtrW(hWnd, GWL.GWL_USERDATA);
                    var gc = GCHandle.FromIntPtr(pointer);
                    var appService = (MainAppService)(gc.Target!);
                    appService._source?.Dispose();
                    appService._source = null;

                    gc.Free();

                    PostQuitMessage(0);
                    break;
                }
        }
        return DefWindowProcW(hWnd, Msg, wParam, lParam);
    }
}