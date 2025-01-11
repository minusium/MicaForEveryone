using MicaForEveryone.App.Services;
using MicaForEveryone.CoreUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Threading;
using System.Threading.Tasks;
using WinRT;

namespace MicaForEveryone.App;

class Program
{
    [STAThread]
    public static async Task Main(string[] _)
    {
        ComWrappersSupport.InitializeComWrappers();

        bool isRedirect = await DecideRedirectionAsync();
        if (!isRedirect)
        {

            Microsoft.UI.Xaml.Application.Start((p) =>
            {
                var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                new App();
            });
        }
    }

    private static async Task<bool> DecideRedirectionAsync()
    {
        bool isRedirect = false;
        AppActivationArguments args = AppInstance.GetCurrent().GetActivatedEventArgs();
        AppInstance keyInstance = AppInstance.FindOrRegisterForKey("MicaForEveryone");

        if (keyInstance.IsCurrent)
        {
            AppInstance.GetCurrent().Activated +=Program_Activated;
        }
        else
        {
            isRedirect = true;
            await keyInstance.RedirectActivationToAsync(args);
        }
        return isRedirect;
    }

    private static async void Program_Activated(object? sender, AppActivationArguments e)
    {
        if (App.Services.GetService<IDispatchingService>() is IDispatchingService dispatcher)
            await dispatcher.YieldAsync();
        App.Services.GetService<MainAppService>()?.ActivateSettings();
    }
}
