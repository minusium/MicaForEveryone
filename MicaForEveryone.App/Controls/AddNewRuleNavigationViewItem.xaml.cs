using MicaForEveryone.App.Views;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace MicaForEveryone.App.Controls;

public sealed partial class AddNewRuleNavigationViewItem : NavigationViewItem
{
    public AddNewRuleNavigationViewItem()
    {
        InitializeComponent();
        AddHandler(PointerReleasedEvent, new PointerEventHandler(CustomPointerReleased), true);
    }

    private void CustomPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        ContextFlyout?.ShowAt(this);
    }

    private void AddProcessRuleMenuFlyoutItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        AddProcessRuleContentDialog addProcessRuleContentDialog = new();
        addProcessRuleContentDialog.XamlRoot = XamlRoot;
        _ = addProcessRuleContentDialog.ShowAsync();
    }

    private void AddClassRuleMenuFlyoutItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        AddClassRuleContentDialog addClassRuleContentDialog = new();
        addClassRuleContentDialog.XamlRoot = XamlRoot;
        _ = addClassRuleContentDialog.ShowAsync();
    }
}