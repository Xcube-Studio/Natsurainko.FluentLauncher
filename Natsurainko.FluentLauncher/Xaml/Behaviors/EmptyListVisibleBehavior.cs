using Microsoft.UI.Xaml;
using Microsoft.Xaml.Interactivity;
using System.Collections;
using System.Collections.Specialized;

#nullable disable
namespace Natsurainko.FluentLauncher.Xaml.Behaviors;

public class EmptyListVisibleBehavior : DependencyObject, IBehavior
{
    public DependencyObject AssociatedObject { get; set; }

    public bool IsObservableCollection { get; set; }

    public IList ItemsSource
    {
        get { return (IList)GetValue(ItemsSourceProperty); }
        set { SetValue(ItemsSourceProperty, value); }
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register("ItemsSource", typeof(IList), typeof(EmptyListVisibleBehavior), new PropertyMetadata(default, OnItemsSourceChanged));

    public void Attach(DependencyObject associatedObject)
    {
        if (associatedObject is FrameworkElement uIElement)
        {
            AssociatedObject = uIElement;

            uIElement.Loaded += UIElement_Loaded;
            uIElement.Unloaded += UIElement_Unloaded;
        }
        else return;
    }

    public void Detach()
    {

    }

    private void UIElement_Unloaded(object sender, RoutedEventArgs e)
    {
        if (IsObservableCollection)
        {
            var notifyCollection = (ItemsSource as INotifyCollectionChanged);
            notifyCollection.CollectionChanged -= EmptyListVisibleBehavior_CollectionChanged;
        }
    }

    private void UIElement_Loaded(object sender, RoutedEventArgs e)
    {
        if (IsObservableCollection)
        {
            var notifyCollection = (ItemsSource as INotifyCollectionChanged);
            notifyCollection.CollectionChanged += EmptyListVisibleBehavior_CollectionChanged;
        }

        UpdateVisibility();
    }

    private void EmptyListVisibleBehavior_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        => UpdateVisibility();

    public static void OnItemsSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArg)
    {
        var behavior = dependencyObject as EmptyListVisibleBehavior;
        behavior.UpdateVisibility();
    }

    public void UpdateVisibility()
    {
        var uIElement = AssociatedObject as FrameworkElement;

        if (uIElement != null)
            uIElement.Visibility = (ItemsSource == null || ItemsSource.Count == 0)
                ? Visibility.Visible
                : Visibility.Collapsed;
    }
}
