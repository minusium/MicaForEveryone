using MicaForEveryone.CoreUI;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace MicaForEveryone.App.Services;

public class PackagedStartupService : IStartupService
{
    StartupTask? _task;
    const string StartupTaskName = "MicaForEveryone2";

    public async Task SetStartupEnabledAsync(bool enabled)
    {
        _task ??= await StartupTask.GetAsync(StartupTaskName);
        if (enabled)
        {
            await _task.RequestEnableAsync();
        }
        else
        {
            _task.Disable();
        }
    }

    public async Task<bool> GetStartupAvailableAsync()
    {
        _task ??= await StartupTask.GetAsync(StartupTaskName);
        return _task.State != StartupTaskState.DisabledByPolicy && _task.State != StartupTaskState.DisabledByUser;
    }

    public async Task<bool> GetStartupEnabledAsync()
    {
        _task ??= await StartupTask.GetAsync(StartupTaskName);
        return _task.State == StartupTaskState.Enabled;
    }
}