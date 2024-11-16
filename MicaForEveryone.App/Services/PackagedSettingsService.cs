using MicaForEveryone.App.Helpers;
using MicaForEveryone.CoreUI;
using MicaForEveryone.Models;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace MicaForEveryone.App.Services;

public sealed partial class PackagedSettingsService : ISettingsService
{
    private SettingsFileModel? _settings = new() { Rules = new() };

    public SettingsFileModel? Settings
    {
        get => _settings;
        set
        {
            if (_settings != value)
            {
                _settings = value;
                PropertyChanged?.Invoke(this, new(nameof(Settings)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private const string SettingsFileName = "settings.json";

    private StorageFile? settingsFile;

    private WinRTFileSystemWatcher? watcher;

    private bool recentWriteDueToApp = false;

    private IDispatchingService _dispatching;

    private SemaphoreSlim semaphore = new(1, 1);

    public PackagedSettingsService(IDispatchingService dispatching)
    {
        _dispatching = dispatching;
    }

    public async Task InitializeAsync()
    {
        var folder = ApplicationData.Current.LocalFolder;
        var file = await folder.TryGetItemAsync(SettingsFileName);
        if (file is null)
        {
            StorageFile defaultFile = await StorageFile.GetFileFromApplicationUriAsync(new("ms-appx:///Assets/default.json"));
            await defaultFile.CopyAsync(ApplicationData.Current.LocalFolder, SettingsFileName);
        }
        settingsFile = await folder.GetFileAsync(SettingsFileName);
        using Stream settingsStream = await settingsFile.OpenStreamForReadAsync();
        Settings = await JsonSerializer.DeserializeAsync(settingsStream, MFESerializerContext.Default.SettingsFileModel);
        watcher = new(folder, WinRTFileSystemWatcher.NotifyFilters.FileName | WinRTFileSystemWatcher.NotifyFilters.LastWrite | WinRTFileSystemWatcher.NotifyFilters.Size, false);
        watcher.Changed += Watcher_Changed;
    }

    private void Watcher_Changed(WinRTFileSystemWatcher watcher, WinRTFileSystemWatcher.FileAction action, string fileName)
    {
        _ = WatcherChangedAsync(fileName);
    }

    private async Task WatcherChangedAsync(string name)
    {
        if (name != string.Empty && !name.Equals(SettingsFileName, StringComparison.CurrentCultureIgnoreCase))
            return;

        if (semaphore.CurrentCount == 0)
            return;

        await semaphore.WaitAsync();

        await Task.Delay(200);

        if (recentWriteDueToApp == true)
        {
            recentWriteDueToApp = false;
            goto RELEASE_SEMAPHORE;
        }

        await _dispatching.YieldAsync();

        // TODO: Finish this
        using (Stream settingsStream = await settingsFile!.OpenStreamForReadAsync())
        {
            Settings = await JsonSerializer.DeserializeAsync(settingsStream, MFESerializerContext.Default.SettingsFileModel);
        }
        Debug.WriteLine("Reloaded");

RELEASE_SEMAPHORE:
        semaphore.Release();
    }

    public async Task SaveAsync()
    {
        using (Stream settingsStream = (await settingsFile!.OpenAsync(FileAccessMode.ReadWrite, StorageOpenOptions.AllowOnlyReaders)).AsStreamForWrite())
        {
            recentWriteDueToApp = true;
            settingsStream.SetLength(0);
            await JsonSerializer.SerializeAsync(settingsStream!, Settings!, MFESerializerContext.Default.SettingsFileModel);
        }
    }

    public void Dispose()
    {
        watcher?.Dispose();
    }
}

[JsonSerializable(typeof(SettingsFileModel))]
[JsonSourceGenerationOptions(WriteIndented = false, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UseStringEnumConverter = true)]
partial class MFESerializerContext : JsonSerializerContext
{

}