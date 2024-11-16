using MicaForEveryone.CoreUI;
using MicaForEveryone.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using static MicaForEveryone.PInvoke.Generic;
using static MicaForEveryone.PInvoke.Windowing;
using static MicaForEveryone.PInvoke.Events;
using static MicaForEveryone.PInvoke.Modules;
using System;

namespace MicaForEveryone.App.Services;

public sealed class RuleService : IRuleService
{
    private readonly ISettingsService _settingsService;
    private HWINEVENTHOOK _eventHookHandler;

    public RuleService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    private static int _currentSession;

    public unsafe void Initialize()
    {
        _eventHookHandler = SetWinEventHook(EVENT_OBJECT_SHOW, EVENT_OBJECT_SHOW, HINSTANCE.NULL, &NewWindowShown, 0, 0, WINEVENT_OUTOFCONTEXT);
        _settingsService.PropertyChanged += _settingsService_PropertyChanged;
    }

    private void _settingsService_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        _ = ApplyRulesToAllWindowsAsync();
    }

    [UnmanagedCallersOnly]
    private static void NewWindowShown(HWINEVENTHOOK handler, uint winEvent, HWND hWnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime)
    {
        async Task NewWindowShowHandlerAsync(IRuleService service, HWND hwnd)
        {
            if (!IsWindowEligible(hwnd))
                await Task.Delay(10);
            await service.ApplyRuleToWindowAsync(hwnd);
        }

        _ = NewWindowShowHandlerAsync(App.Services.GetRequiredService<IRuleService>(), hWnd);
    }

    public async Task ApplyRulesToAllWindowsAsync()
    {
        // Switch to a background thread, if we are not already in one.
        await TaskScheduler.Default;

        // Increase the session count to prevent concurrency issues,
        // that is, if the user changes the settings while we are applying the rules.
        // This tells the existing procedure to cancel the existing operation.
        int incrementedValue = Interlocked.Increment(ref _currentSession);

        unsafe
        {
            EnumWindows(&EnumWindowsProc, new(incrementedValue));
        }
    }

    [UnmanagedCallersOnly]
    private static BOOL EnumWindowsProc(HWND hWnd, LPARAM lParam)
    {
        if (Volatile.Read(ref _currentSession) != lParam.Value.ToInt32())
            // User changed the settings, cancel the operation.
            return BOOL.FALSE;

        _ = App.Services.GetRequiredService<IRuleService>().ApplyRuleToWindowAsync(hWnd);

        return BOOL.TRUE;
    }

    private static unsafe bool IsWindowEligible(HWND hWnd)
    {
        if (!IsWindowVisible(hWnd))
            return false;

        WindowStylesEx styleEx = (WindowStylesEx)GetWindowLongPtrW(hWnd, WindowLongIndex.GWL_EXSTYLE);

        WindowStyles style = (WindowStyles)GetWindowLongPtrW(hWnd, WindowLongIndex.GWL_STYLE);

        if (styleEx.HasFlag(WindowStylesEx.WS_EX_NOACTIVATE))
            return false;

        if (IsTopLevelWindow(hWnd) == BOOL.FALSE)
            return false;

        bool hasTitleBar = style.HasFlag(WindowStyles.WS_BORDER) && style.HasFlag(WindowStyles.WS_DLGFRAME);

        if (styleEx.HasFlag(WindowStylesEx.WS_EX_TOOLWINDOW) && !hasTitleBar)
            return false;

        if (style.HasFlag(WindowStyles.WS_POPUP) & !hasTitleBar)
            return false;

        return true;
    }

    public Task ApplyRuleToWindowAsync(HWND hWnd)
    {
        if (!IsWindowEligible(hWnd))
            return Task.CompletedTask;

        Rule mostApplicableRule = _settingsService.Settings!.Rules.Where(f => f.IsRuleApplicable(hWnd)).OrderByDescending(f => f is not GlobalRule).First();

        if (mostApplicableRule.BackdropPreference != BackdropType.Default)
        {
            uint bp = (uint)mostApplicableRule.BackdropPreference;
            unsafe
            {
                DwmSetWindowAttribute(hWnd, DWMWA_SYSTEMBACKDROP_TYPE, &bp, sizeof(uint));
            }
        }

        return Task.CompletedTask;
    }
}
