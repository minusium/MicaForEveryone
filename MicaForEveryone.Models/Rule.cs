using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;
using static MicaForEveryone.PInvoke.Windowing;

namespace MicaForEveryone.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(GlobalRule), "global")]
[JsonDerivedType(typeof(ProcessRule), "process")]
[JsonDerivedType(typeof(ClassRule), "class")]
public abstract partial class Rule: ObservableObject, IEquatable<Rule>
{
    [ObservableProperty]
    public partial TitleBarColorMode TitleBarColor { get; set; }

    [ObservableProperty]
    public partial BackdropType BackdropPreference { get; set; }

    [ObservableProperty]
    public partial CornerPreference CornerPreference { get; set; }

    [ObservableProperty]
    public partial bool ExtendFrameIntoClientArea { get; set; }

    [ObservableProperty]
    public partial bool EnableBlurBehind { get; set; }

    [ObservableProperty]
    public partial string? TitleBarColorCode { get; set; }

    public virtual bool Equals(Rule? other)
    {
        return TitleBarColor == other!.TitleBarColor
            && BackdropPreference == other.BackdropPreference
            && CornerPreference == other.CornerPreference
            && ExtendFrameIntoClientArea == other.ExtendFrameIntoClientArea
            && EnableBlurBehind == other.EnableBlurBehind
            && TitleBarColorCode == other.TitleBarColorCode;
    }

    public abstract bool IsRuleApplicable(HWND hWnd);
}
