using System.Diagnostics;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;

namespace MicaForEveryone.Models;

public sealed partial class ProcessRule : Rule
{
    public required string ProcessName { get; set; }

    public override bool Equals(Rule? other)
    {
        return other is not null 
            && other is ProcessRule pRule 
            && ProcessName.Equals(pRule.ProcessName, StringComparison.CurrentCultureIgnoreCase) 
            && base.Equals(other);
    }

    public override unsafe bool IsRuleApplicable(HWND hWnd)
    {
        uint processId = 0;
        if (GetWindowThreadProcessId(hWnd, &processId) == 0)
        {
            return false;
        }
        Process proc = Process.GetProcessById((int)processId);
        string procName = proc.ProcessName;
        return procName.Equals(ProcessName, StringComparison.CurrentCultureIgnoreCase);
    }
}