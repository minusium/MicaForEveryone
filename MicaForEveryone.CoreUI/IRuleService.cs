namespace MicaForEveryone.CoreUI;

public interface IRuleService
{
    void Initialize();

    Task ApplyRulesToAllWindowsAsync();

    Task ApplyRuleToWindowAsync(TerraFX.Interop.Windows.HWND hwnd);
}
