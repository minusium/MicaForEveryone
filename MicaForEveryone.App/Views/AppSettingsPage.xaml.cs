using MicaForEveryone.App.ViewModels;
using MicaForEveryone.CoreUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MicaForEveryone.App.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AppSettingsPage : Page
{
    private AppSettingsPageViewModel ViewModel { get; }
    private IStartupService StartupService { get; }

    public AppSettingsPage()
    {
        this.InitializeComponent();

        ViewModel = App.Services.GetRequiredService<AppSettingsPageViewModel>();
        StartupService = App.Services.GetRequiredService<IStartupService>();

        _ = PopulateStartupToggle();
    }

    private async Task PopulateStartupToggle()
    {
        StartupToggle.IsEnabled = await StartupService.GetStartupAvailableAsync();
        StartupToggle.IsOn = await StartupService.GetStartupEnabledAsync();
    }

    private async void StartupToggle_Toggled(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (StartupToggle.IsOn != await StartupService.GetStartupEnabledAsync())
        {
            await StartupService.SetStartupEnabledAsync(StartupToggle.IsOn);
            await PopulateStartupToggle();
        }
    }
}
