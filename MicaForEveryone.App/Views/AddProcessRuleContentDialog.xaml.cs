using MicaForEveryone.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;

namespace MicaForEveryone.App.Views;

public sealed partial class AddProcessRuleContentDialog : ContentDialog
{
    private AddProcessRuleContentDialogViewModel ViewModel { get; }

    public AddProcessRuleContentDialog()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<AddProcessRuleContentDialogViewModel>();
    }

    private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            ViewModel.RequestSuggestions();
        }
    }

    private unsafe void WindowPickerButton_WindowChanged(Controls.WindowPickerButton sender, HWND window)
    {
        if (window == HWND.NULL)
        {
            ViewModel.ProcessName = string.Empty;
            return;
        }
        uint procId;
        if (GetWindowThreadProcessId(window, &procId) == 0)
        {
            return;
        }
        Process proc = Process.GetProcessById((int)procId);
        ViewModel.ProcessName = proc.ProcessName;
    }
}
