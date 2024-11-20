using MicaForEveryone.Core.Interfaces;
using MicaForEveryone.Core.Ui.Models;
using MicaForEveryone.Core.Ui.ViewModels;
using XclParser.Reflection;

namespace MicaForEveryone.Core.Models
{
    [XclType(TypeName = "Global")]
    public class GlobalRule : IRule
    {
        public string Name => "Global";

        public int Priority => 2;

        [XclField]
        public TitlebarColorMode TitleBarColor { get; set; }

        [XclField]
        public BackdropType BackdropPreference { get; set; }

        [XclField]
        public CornerPreference CornerPreference { get; set; }

        [XclField]
        public bool ExtendFrameIntoClientArea { get; set; }

        [XclField]
        public bool EnableBlurBehind { get; set; }

        [XclField]
        public string CaptionColor { get; set; } = string.Empty;

        [XclField]
        public string CaptionTextColor { get; set; } = string.Empty;

        [XclField]
        public string BorderColor { get; set; } = string.Empty;

        public bool IsApplicable(TargetWindow target) => true;

        public override string ToString() => Name;

        public RulePaneItem GetPaneItem(ISettingsViewModel parent, IRuleSettingsViewModel viewModel)
        {
            viewModel.ParentViewModel = parent;
            viewModel.Rule = this;
            return new RulePaneItem("", PaneItemType.Global, viewModel);
        }
    }
}
