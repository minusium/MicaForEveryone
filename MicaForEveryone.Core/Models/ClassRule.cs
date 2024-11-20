using XclParser.Reflection;

using MicaForEveryone.Core.Interfaces;
using MicaForEveryone.Core.Ui.Models;
using MicaForEveryone.Core.Ui.ViewModels;

namespace MicaForEveryone.Core.Models
{
    [XclType(TypeName = "Class")]
    public class ClassRule : IRule
    {
        [XclConstructor]
        public ClassRule(string className)
        {
            ClassName = className;
        }

        public string Name => $"Class({ClassName})";

        public int Priority => 0;

        [XclParameter]
        public string ClassName { get; }

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

        public bool IsApplicable(TargetWindow target) =>
            target.ClassName == ClassName;

        public override string ToString() => Name;

        public RulePaneItem GetPaneItem(ISettingsViewModel parent, IRuleSettingsViewModel viewModel)
        {
            viewModel.ParentViewModel = parent;
            viewModel.Rule = this;
            return new RulePaneItem(ClassName, PaneItemType.Class, viewModel);
        }
    }
}
