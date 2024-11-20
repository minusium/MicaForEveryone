using MicaForEveryone.Core.Models;
using MicaForEveryone.Core.Ui.Models;
using MicaForEveryone.Core.Ui.ViewModels;

namespace MicaForEveryone.Core.Interfaces;

public interface IRule
{
    string Name { get; }

    int Priority { get; }

    bool IsApplicable(TargetWindow target);

    TitlebarColorMode TitleBarColor { get; set; }
    BackdropType BackdropPreference { get; set; }
    CornerPreference CornerPreference { get; set; }
    bool ExtendFrameIntoClientArea { get; set; }
    bool EnableBlurBehind { get; set; }
    string CaptionColor { get; set; }
    string CaptionTextColor { get; set; }
    string BorderColor { get; set; }

    RulePaneItem GetPaneItem(ISettingsViewModel parent, IRuleSettingsViewModel viewModel);
}