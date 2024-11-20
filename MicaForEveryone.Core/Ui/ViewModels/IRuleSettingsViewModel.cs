using System.ComponentModel;

using MicaForEveryone.Core.Interfaces;
using MicaForEveryone.Core.Models;

namespace MicaForEveryone.Core.Ui.ViewModels
{
    public interface IRuleSettingsViewModel : INotifyPropertyChanged
    {
        BackdropType BackdropType { get; set; }
        TitlebarColorMode TitlebarColor { get; set; }
        CornerPreference CornerPreference { get; set; }
        bool ExtendFrameIntoClientArea { get; set; }
        bool EnableBlurBehind { get; set; }
        string CaptionColor { get; set; }
        string CaptionTextColor { get; set; }
        string BorderColor { get; set; }
        ISettingsViewModel ParentViewModel { get; set; }
        IRule? Rule { get; set; }
    }
}
