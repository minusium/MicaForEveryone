namespace MicaForEveryone.CoreUI;

public interface IStartupService
{
    Task<bool> GetStartupEnabledAsync();

    Task<bool> GetStartupAvailableAsync();

    Task SetStartupEnabledAsync(bool enabled);
}