using MicaForEveryone.CoreUI;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace MicaForEveryone.App.Services;

internal class PackagedUpdateCheckerService : IUpdateCheckerService
{
    public async Task<UpdateAvailability> CheckForUpdateAsync()
    {
        PackageUpdateAvailabilityResult updateResult = await Package.Current.CheckUpdateAvailabilityAsync();
        return updateResult.Availability switch
        {
            PackageUpdateAvailability.Available => UpdateAvailability.UpdateAvailable,
            PackageUpdateAvailability.Required => UpdateAvailability.UpdateAvailable,
            PackageUpdateAvailability.Error => UpdateAvailability.Error,
            _ => UpdateAvailability.NoUpdate
        };
    }
}
