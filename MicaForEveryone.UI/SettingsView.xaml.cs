using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MicaForEveryone.UI.Models;
using Microsoft.UI.Xaml.Controls;
using MicaForEveryone.Core.Ui.ViewModels;

namespace MicaForEveryone.UI
{
    public sealed partial class SettingsView
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        public ISettingsViewModel ViewModel { get; set; }

        internal Contributor[] Contributors { get; } =
        {
            new Contributor("Dongle the Gadget", "https://github.com/dongle-the-gadget", null),
            new Contributor("sitiom", "https://github.com/sitiom", null),
            new Contributor("krlvm", "https://github.com/krlvm", null),
            new Contributor("TheXDS", "https://github.com/TheXDS", null),
        };

        internal Contributor[] Translators { get; } =
        {
            new Contributor("Mehrab Poladov", "https://github.com/thepoladov13", "az"),
            new Contributor("maggster165", "https://github.com/maggster165", "cs"),
            new Contributor("WinExperiments", "https://github.com/WinExperiments", "ar"),
            new Contributor("Fadexz", "https://github.com/Fadexz", "GB"),
            new Contributor("uDEV2019", "https://github.com/uDEV2019", "de"),
            new Contributor("Amaro Martínez", "https://github.com/xoascf", "es"),
            new Contributor("SaiyajinK", "https://github.com/SaiyajinK", "fr"),
            new Contributor("Zan1456", "https://github.com/Zan1456", "hu"),
            new Contributor("seanardhana", "https://github.com/seanardhana", "id"),
            new Contributor("GID0317", "https://github.com/GID0317", "id"),
            new Contributor("alessandrocaseti", "https://github.com/alessandrocaseti", "it"),
            new Contributor("A-Loot", "https://github.com/A-Loot", "it"),
            new Contributor("maboroshin", "https://github.com/maboroshin", "ja"),
            new Contributor("RTFTR", "https://github.com/RTFTR", "ko"),
            new Contributor("toineenzo", "https://github.com/toineenzo", "nl"),
            new Contributor("Piteriuz", "https://github.com/Piteriuz", "pl"),
            new Contributor("Douglas Vianna", "https://github.com/dgsmiley18", "pt-br"),
            new Contributor("DavidJoacaRo", "https://github.com/DavidJoacaRo", "ro"),
            new Contributor("Danik2343", "https://github.com/Danik2343", "ru"),
            new Contributor("krlvm", "https://github.com/krlvm", "ru"),
            new Contributor("bzzrak", "https://github.com/bzzrak", "sr"),
            new Contributor("Meriç Bağlayan", "https://github.com/baglayan", "tr"),
            new Contributor("Phyan", "https://github.com/Phyan", "uk"),
            new Contributor("lowl1f3", "https://github.com/lowl1f3", "uk"),
            new Contributor("ChefKozaki", "https://github.com/chefkozaki", "vi"),
            new Contributor("AndromedaMelody", "https://github.com/AndromedaMelody", "zh-Hans"),
            new Contributor("FrzMtrsprt", "https://github.com/FrzMtrsprt", "zh-Hans"),
            new Contributor("MW1Z", "https://github.com/MW1Z", "zh-Hant"),
            new Contributor("宥叡", "https://github.com/jay900604", "zh-Hant"),
            new Contributor("flandretw", "https://github.com/flandretw", "zh-Hant"),
			new Contributor("trlef19", "https://github.com/trlef19", "el"),
        };

        private void ListView_Loaded(object sender, RoutedEventArgs args)
        {
            ((ListView)sender).Focus(FocusState.Programmatic);
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs args)
        {
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
        }

        private void AddRuleAppBarButton_OnLoaded(object sender, RoutedEventArgs e)
        {
            ToolTipService.SetToolTip((DependencyObject)sender, new ResourceLoader().GetString("AddRuleAppBarButton/Label"));
        }

        private void RemoveRuleAppBarButton_OnLoaded(object sender, RoutedEventArgs e)
        {
            ToolTipService.SetToolTip((DependencyObject)sender, new ResourceLoader().GetString("RemoveRuleAppBarButton/Label"));
        }
    }
}
