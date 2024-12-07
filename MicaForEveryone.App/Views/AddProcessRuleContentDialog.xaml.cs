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

    private bool capturing = false;

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
        uint procId;
        if (GetWindowThreadProcessId(window, &procId) == 0)
        {
            return;
        }
        Process proc = Process.GetProcessById((int)procId);
        if (proc.ProcessName == Process.GetCurrentProcess().ProcessName)
        {
            ViewModel.ProcessName = string.Empty;
            return;
        }
        ViewModel.ProcessName = proc.ProcessName;
    }
}
