using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using TerraFX.Interop.Windows;

using static TerraFX.Interop.Windows.Windows;

namespace MicaForEveryone.App.Controls;

public delegate void WindowPickerButtonWindowChangedHandler(WindowPickerButton sender, HWND window);

public sealed partial class WindowPickerButton : Button
{
    private bool capturing = false;

    public static DependencyProperty WindowProperty = DependencyProperty.Register(nameof(Window), typeof(HWND), typeof(WindowPickerButton), new PropertyMetadata(null));

    public event WindowPickerButtonWindowChangedHandler? WindowChanged;

    public HWND Window
    {
        get => (HWND)GetValue(WindowProperty);
        set => SetValue(WindowProperty, value);
    }


    public WindowPickerButton()
    {
        DefaultStyleKey = typeof(WindowPickerButton);
    }

    protected override void OnPointerPressed(PointerRoutedEventArgs e)
    {
        base.OnPointerPressed(e);
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Cross);
        capturing = true;
    }

    protected override void OnPointerReleased(PointerRoutedEventArgs e)
    {
        base.OnPointerReleased(e);
        ProtectedCursor = null;
        capturing = false;
    }

    protected override unsafe void OnPointerMoved(PointerRoutedEventArgs e)
    {
        base.OnPointerMoved(e);

        if (!capturing)
            return;
        POINT point;
        GetCursorPos(&point);
        HWND window = WindowFromPoint(point);
        Window = GetAncestor(window, GA.GA_ROOT);
        WindowChanged?.Invoke(this, Window);
    }
}
