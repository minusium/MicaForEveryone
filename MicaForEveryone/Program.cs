using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using MicaForEveryone.Core.Interfaces;
using MicaForEveryone.Core.Services;
using MicaForEveryone.Core.Ui.ViewModels;
using MicaForEveryone.Interfaces;
using MicaForEveryone.Services;
using MicaForEveryone.ViewModels;
using MicaForEveryone.Win32;

namespace MicaForEveryone
{
    internal static class Program
    {
        public static IServiceProvider Container { get; } = RegisterServices();

        [STAThread]
        public static async Task Main(string[] args)
        {
            AccentColor.InitializeListeners();

            if (Environment.OSVersion.Version.Build < 18362)
            {
                Environment.Exit(1);
                return;
            }

            if (args.Length > 0 && args[0] == "--uninstall")
            {
                UninstallService.Run();
                return;
            }

            var srvLifetime = Container.GetRequiredService<IAppLifeTimeService>();

            if (!srvLifetime.IsFirstInstance())
            {
                srvLifetime.OpenSettingsWindow();
                return;
            }

            await srvLifetime.InitializeRuleServiceAsync();
            
            await srvLifetime.RunViewServiceAsync();
            
            srvLifetime.ShutdownViewService();
            srvLifetime.ShutdownRuleService();
        }
        
        private static IServiceProvider RegisterServices()
        {
            var services = new ServiceCollection();

            services.AddTransient<ISettingsContainer>(container =>
                Application.IsPackaged ? new UwpSettingsContainer()
                    : new RegistrySettingsContainer());
            services.AddTransient<IConfigParser, XclParserService>();
            services.AddTransient<IConfigFile>(container =>
                Application.IsPackaged ? new UwpConfigFile(container.GetService<IConfigParser>())
                    : new Win32ConfigFile(container.GetService<IConfigParser>()));
            services.AddTransient<ITrayIconViewModel, TrayIconViewModel>();
            services.AddTransient<UI.ViewModels.IContentDialogViewModel, ContentDialogViewModel>();
            services.AddTransient<ISettingsViewModel, SettingsViewModel>();
            services.AddTransient<IGeneralSettingsViewModel, GeneralSettingsViewModel>();
            services.AddTransient<IRuleSettingsViewModel, RuleSettingsViewModel>();
            services.AddTransient<IAddProcessRuleViewModel, AddProcessRuleViewModel>();
            services.AddTransient<IAddClassRuleViewModel, AddClassRuleViewModel>();

            services.AddSingleton<IStartupService>(container =>
                Application.IsPackaged ? new UwpStartupService()
                    : new Win32StartupService());
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IUiSettingsService, UiSettingsServiceService>();
            services.AddSingleton<IRuleService, RuleService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IViewService, ViewService>();
            services.AddSingleton<ILanguageService, LanguageService>();
            services.AddSingleton<ITaskSchedulerService, TaskSchedulerService>();
            services.AddSingleton<IAppLifeTimeService, AppLifeTimeService>();

            return services.BuildServiceProvider();
        }
    }
}
