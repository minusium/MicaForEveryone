using CommunityToolkit.Mvvm.ComponentModel;

using MicaForEveryone.Core.Interfaces;
using MicaForEveryone.Core.Models;
using MicaForEveryone.Core.Ui.ViewModels;
using MicaForEveryone.Interfaces;

#nullable enable

namespace MicaForEveryone.ViewModels
{
    internal class RuleSettingsViewModel : ObservableObject, IRuleSettingsViewModel
    {
        private readonly ISettingsService _settingsService;

        public RuleSettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public BackdropType BackdropType
        {
            get => Rule?.BackdropPreference ?? default;
            set
            {
                if (Rule != null && Rule.BackdropPreference != value)
                {
                    Rule.BackdropPreference = value;
                    _settingsService.UpdateRuleAsync(Rule);
                }
            }
        }

        public TitlebarColorMode TitlebarColor
        {
            get => Rule?.TitleBarColor ?? default;
            set
            {
                if (Rule != null && Rule.TitleBarColor != value)
                {
                    Rule.TitleBarColor = value;
                    _settingsService.UpdateRuleAsync(Rule);
                }
            }
        }

        public CornerPreference CornerPreference
        {
            get => Rule?.CornerPreference ?? default;
            set
            {
                if (Rule != null && Rule.CornerPreference != value)
                {
                    Rule.CornerPreference = value;
                    _settingsService.UpdateRuleAsync(Rule);
                }
            }
        }

        public bool ExtendFrameIntoClientArea
        {
            get => Rule?.ExtendFrameIntoClientArea ?? default;
            set
            {
                if (Rule != null && Rule.ExtendFrameIntoClientArea != value)
                {
                    Rule.ExtendFrameIntoClientArea = value;
                    _settingsService.UpdateRuleAsync(Rule);
                }
            }
        }

        public bool EnableBlurBehind
        {
            get => Rule?.EnableBlurBehind ?? default;
            set
            {
                if (Rule != null && Rule.EnableBlurBehind != value)
                {
                    Rule.EnableBlurBehind = value;
                    _settingsService.UpdateRuleAsync(Rule);
                }
            }
        }

        public Core.Ui.ViewModels.ISettingsViewModel? ParentViewModel { get; set; }
        public string CaptionColor {
            get => Rule?.CaptionColor ?? "";
            set {
                if (Rule != null && Rule.CaptionColor != value) {
                    Rule.CaptionColor = value;
                    // _settingsService.CommitChangesAsync(SettingsChangeType.RuleChanged, Rule);
                    _settingsService.UpdateRuleAsync(Rule);
                }
            }
        }

        public string CaptionTextColor {
            get => Rule?.CaptionTextColor ?? "";
            set {
                if (Rule != null && Rule.CaptionTextColor != value) {
                    Rule.CaptionTextColor = value;
                    // _settingsService.CommitChangesAsync(SettingsChangeType.RuleChanged, Rule);
                    _settingsService?.UpdateRuleAsync(Rule);
                }
            }
        }

        public string BorderColor {
            get => Rule?.BorderColor ?? "";
            set {
                if (Rule != null && Rule.BorderColor != value) {
                    Rule.BorderColor = value;
                    // _settingsService.CommitChangesAsync(SettingsChangeType.RuleChanged, Rule);
                    _settingsService?.UpdateRuleAsync(Rule);
                }
            }
        }

        public IRule? Rule { get; set; }
    }
}
