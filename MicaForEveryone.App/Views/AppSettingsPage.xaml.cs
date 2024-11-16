using MicaForEveryone.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MicaForEveryone.App.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AppSettingsPage : Page
{
    private AppSettingsPageViewModel ViewModel { get; }

    public AppSettingsPage()
    {
        this.InitializeComponent();

        ViewModel = App.Services.GetRequiredService<AppSettingsPageViewModel>();
    }
}
