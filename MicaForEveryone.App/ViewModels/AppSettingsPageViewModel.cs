using CommunityToolkit.Mvvm.ComponentModel;
using MicaForEveryone.CoreUI;

namespace MicaForEveryone.App.ViewModels;

public sealed class AppSettingsPageViewModel : ObservableObject
{
    private IVersionInfoService _versionInfoService;

    public string AppVersion => _versionInfoService.GetVersion();
    
    public AppSettingsPageViewModel(IVersionInfoService versionInfoService)
    {
        _versionInfoService = versionInfoService;
    }
}
