using MicaForEveryone.Core.Interfaces;
using MicaForEveryone.Core.Ui.Models;
using MicaForEveryone.Core.Ui.ViewModels;
using XclParser.Reflection;

namespace MicaForEveryone.Core.Models
{
    [XclType(TypeName = "Process")]
    public class ProcessRule : IRule
    {
        [XclConstructor]
        public ProcessRule(string processName)
        {
            ProcessName = processName;
            ProcessNames = processName.Split("|").ToList();
        }

        public string Name => $"Process({ProcessName})";

        public int Priority => 1;

        [XclParameter]
        public string ProcessName { get; }

        public List<string> ProcessNames { get; }

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

        public bool IsApplicable(TargetWindow target) => ProcessNames.Contains(target.ProcessName);

        public override string ToString() => Name;

        public RulePaneItem GetPaneItem(ISettingsViewModel parent, IRuleSettingsViewModel viewModel)
        {
            viewModel.ParentViewModel = parent;
            viewModel.Rule = this;
            return new RulePaneItem(ProcessName, PaneItemType.Process, viewModel);
        }
    }
}
