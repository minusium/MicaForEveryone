using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MicaForEveryone.CoreUI;
using System.Threading.Tasks;

namespace MicaForEveryone.App.ViewModels;

public sealed partial class AppSettingsPageViewModel : ObservableObject
{
    private IVersionInfoService _versionInfoService;

    private IUpdateCheckerService _updateCheckService;

    public string AppVersion => _versionInfoService.GetVersion();

    [ObservableProperty]
    public partial bool UpdateChecked { get; set; }

    [ObservableProperty]
    public partial UpdateAvailability UpdateAvailability { get; set; }

    public AppSettingsPageViewModel(IVersionInfoService versionInfoService, IUpdateCheckerService updateCheckerService)
    {
        _versionInfoService = versionInfoService;
        _updateCheckService = updateCheckerService;
    }

    [RelayCommand]
    private async Task CheckForUpdatesAsync()
    {
        UpdateAvailability = await _updateCheckService.CheckForUpdateAsync();
        UpdateChecked = true;
    }
}
