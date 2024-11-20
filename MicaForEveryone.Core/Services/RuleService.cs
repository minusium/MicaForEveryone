using MicaForEveryone.Core.Interfaces;
using MicaForEveryone.Core.Models;
using MicaForEveryone.Win32;
using MicaForEveryone.Win32.Events;

namespace MicaForEveryone.Core.Services
{
    public class RuleService : IRuleService
    {
        private readonly ISettingsService _settingsService;

        private readonly Mutex _applyAllWindowsMutex = new();

        public RuleService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _settingsService.RuleAdded += SettingsService_Changed;
            _settingsService.RuleRemoved += SettingsService_Changed;
            _settingsService.RuleChanged += SettingsService_Changed;
            _settingsService.ConfigFileReloaded += SettingsService_Changed;
            _settingsService.ConfigFilePathChanged += SettingsService_Changed;
            AccentColor.AccentColorChanged += Accent_Changed;
        }

        ~RuleService()
        {
            _settingsService.RuleAdded -= SettingsService_Changed;
            _settingsService.RuleRemoved -= SettingsService_Changed;
            _settingsService.RuleChanged -= SettingsService_Changed;
            _settingsService.ConfigFileReloaded -= SettingsService_Changed;
            _settingsService.ConfigFilePathChanged -= SettingsService_Changed;
            AccentColor.AccentColorChanged -= Accent_Changed;
            Dispose(false);
        }

        public TitlebarColorMode SystemTitlebarColorMode { get; set; }

        public bool IsRunning { get; private set; }

        public void StartService()
        {
            if (IsRunning) return;
            IsRunning = true;
            var winEvent = new WindowOpenedEvent();
            winEvent.Handler += WinEvent_Handler;
            WinEventManager.AddEventHandler(winEvent);
        }

        public void StopService()
        {
            WinEventManager.RemoveAll();
            IsRunning = false;
        }

        public void MatchAndApplyRuleToWindow(TargetWindow target)
        {
            try
            {
                var applicableRules = _settingsService.Rules.Where(rule => rule.IsApplicable(target)).OrderBy(rule => rule.Priority);
                var rule = applicableRules.FirstOrDefault();

                if (rule == null)
                    return;

                target.ApplyRule(rule, SystemTitlebarColorMode);
            }
#if DEBUG
            catch (Exception ex)
            {
				System.Diagnostics.Debug.WriteLine(ex);
				// System.Diagnostics.Debugger.Break();
            }
#else
            catch
            {
                // ignore
            }
#endif
        }

        public Task MatchAndApplyRuleToAllWindowsAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    _applyAllWindowsMutex.WaitOne();
                    Window.GetDesktopWindow().ForEachChild(window =>
                    {
                        if (!window.IsVisible())
                            return;

                        if (!window.IsWindowPatternValid())
                            return;

                        if (window.InstanceHandle == Application.InstanceHandle)
                            return; // ignore windows of current instance

                        MatchAndApplyRuleToWindow(TargetWindow.FromWindow(window));
                    });
                }
                finally
                {
                    _applyAllWindowsMutex.ReleaseMutex();
                }
            });

        }

        private void SettingsService_Changed(object? sender, EventArgs args)
        {
            _ = MatchAndApplyRuleToAllWindowsAsync();
        }
        
        private void WinEvent_Handler(object sender, WindowOpenedEventArgs args)
        {
			if (args.Window.InstanceHandle == Application.InstanceHandle)
				return; // ignore windows of current instance

			var target = TargetWindow.FromWindow(args.Window);
			MatchAndApplyRuleToWindow(target);
		}

        public void Accent_Changed(object? sender, EventArgs args)
        {
            SettingsService_Changed(sender, args);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing) return;
            _applyAllWindowsMutex.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
