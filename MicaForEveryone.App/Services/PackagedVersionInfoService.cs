using MicaForEveryone.CoreUI;
using Windows.ApplicationModel;

namespace MicaForEveryone.App.Services;

public sealed class PackagedVersionInfoService : IVersionInfoService
{
    public string GetVersion()
    {
        var version = Package.Current.Id.Version;
        return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}
