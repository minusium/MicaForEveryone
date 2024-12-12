using MicaForEveryone.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;

using static TerraFX.Interop.Windows.Windows;

namespace MicaForEveryone.App.Views;

public sealed partial class AddClassRuleContentDialog : ContentDialog
{
    private AddClassRuleContentDialogViewModel ViewModel { get; }

    public AddClassRuleContentDialog()
    {
        this.InitializeComponent();

        ViewModel = App.Services.GetRequiredService<AddClassRuleContentDialogViewModel>();
    }

    private unsafe void WindowPickerButton_WindowChanged(Controls.WindowPickerButton sender, HWND window)
    {
        if (window == HWND.NULL)
        {
            ViewModel.ClassName = string.Empty;
            return;
        }
        char* lpClassName = stackalloc char[256];
        if (GetClassNameW(window, lpClassName, 256) == 0)
        {
            return;
        }
        ReadOnlySpan<char> className = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(lpClassName);
        ViewModel.ClassName = className.ToString();
    }
}
