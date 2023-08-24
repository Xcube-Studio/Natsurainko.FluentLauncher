using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Xaml.Interactivity;

namespace Natsurainko.FluentLauncher.Utils.Xaml.Behaviors;

public class PointerOverVisibleBehavior : DependencyObject, IBehavior
{
    public DependencyObject AssociatedObject { get; set; }

    public bool UseMenuFlyout { get; set; } = false;

    public string TargetElementName { get; set; }

    public string MenuFlyoutElementName { get; set; }

    private UIElement TargetElement { get; set; }

    private MenuFlyout MenuFlyout { get; set; }

    private bool IsMenuOpen
    {
        get { return (bool)GetValue(IsMenuOpenProperty); }
        set { SetValue(IsMenuOpenProperty, value); }
    }

    public static readonly DependencyProperty IsMenuOpenProperty =
        DependencyProperty.Register("IsMenuOpen", typeof(bool), typeof(PointerOverVisibleBehavior), new PropertyMetadata(false, OnIsMenuOpenChanged));

    public void Attach(DependencyObject associatedObject)
    {
        if (associatedObject is FrameworkElement uIElement)
        {
            AssociatedObject = uIElement;

            TargetElement = uIElement.FindName(TargetElementName) as UIElement;

            uIElement.PointerEntered += UIElement_PointerEntered;
            uIElement.PointerExited += UIElement_PointerExited;
        }
        else return;

        if (UseMenuFlyout)
        {
            MenuFlyout = uIElement.FindName(MenuFlyoutElementName) as MenuFlyout;

            if (MenuFlyout != null)
            {
                MenuFlyout.Closed += MenuFlyout_Closed;
                MenuFlyout.Opened += MenuFlyout_Opened;
            }
        }

        if (TargetElement != null)
            TargetElement.Visibility = Visibility.Collapsed;
    }

    private void MenuFlyout_Closed(object sender, object e) => IsMenuOpen = false;

    private void MenuFlyout_Opened(object sender, object e) => IsMenuOpen = true;

    public void Detach()
    {

    }

    private void UIElement_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (UseMenuFlyout && IsMenuOpen)
            return;

        if (TargetElement != null)
            TargetElement.Visibility = Visibility.Collapsed;
    }

    private void UIElement_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (TargetElement != null)
            TargetElement.Visibility = Visibility.Visible;
    }

    private static void OnIsMenuOpenChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
    {
        var behavior = dependencyObject as PointerOverVisibleBehavior;

        if (behavior.UseMenuFlyout && !behavior.IsMenuOpen)
        {
            if (behavior.TargetElement.Visibility == Visibility.Visible)
                behavior.TargetElement.Visibility = Visibility.Collapsed;
        }
    }
}
