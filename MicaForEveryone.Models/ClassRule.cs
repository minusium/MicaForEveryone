using CommunityToolkit.Mvvm.ComponentModel;
using MicaForEveryone.PInvoke;
using System.Runtime.InteropServices;

namespace MicaForEveryone.Models;

public sealed partial class ClassRule : Rule
{
    public required string ClassName { get; set; }

    public override bool Equals(Rule? other)
    {
        return other is not null
            && other is ClassRule cRule
            && ClassName.Equals(cRule.ClassName, StringComparison.CurrentCultureIgnoreCase)
            && base.Equals(other);
    }

    public override unsafe bool IsRuleApplicable(Windowing.HWND hWnd)
    {
        char* lpClassName = stackalloc char[256];
        if (Windowing.GetClassNameW(hWnd, lpClassName, 256) == 0)
        {
            return false;
        }
        ReadOnlySpan<char> className = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(lpClassName);
        return className.Equals(ClassName.AsSpan(), StringComparison.CurrentCultureIgnoreCase);
    }
}