using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.Xaml.Interactivity;
using Windows.Foundation.Collections;

namespace Natsurainko.FluentLauncher.Behaviors;

class SetComboBoxWidthFromItemsBehavior : Behavior<ComboBox>
{
    #region SetWidthFromItemsProperty

    public static readonly DependencyProperty SetWidthFromItemsProperty =
        DependencyProperty.RegisterAttached
        (
            "SetWidthFromItems",
            typeof(bool),
            typeof(SetComboBoxWidthFromItemsBehavior),
            new PropertyMetadata(false, OnSetWidthFromItemsPropertyChanged)
        );

    public bool SetWidthFromItems 
    { 
        get => (bool)GetValue(SetWidthFromItemsProperty); 
        set => SetValue(SetWidthFromItemsProperty, value);
    }

    #endregion

    #region AutoMaxWidthProperty

    public static readonly DependencyProperty AutoMaxWidthProperty =
        DependencyProperty.RegisterAttached
        (
            "AutoMaxWidthProperty",
            typeof(bool),
            typeof(SetComboBoxWidthFromItemsBehavior),
            new PropertyMetadata(null, OnAutoMaxWidthPropertyChanged)
        );

    public bool AutoMaxWidth
    {
        get => (bool)GetValue(AutoMaxWidthProperty);
        set => SetValue(AutoMaxWidthProperty, value);
    }

    #endregion

    FrameworkElement container = null;

    private static void OnSetWidthFromItemsPropertyChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        SetComboBoxWidthFromItemsBehavior behavior = (SetComboBoxWidthFromItemsBehavior)d;
        ComboBox comboBox = behavior.AssociatedObject;

        bool newValue = (bool)e.NewValue;
        bool oldValue = (bool)e.OldValue;

        if (comboBox != null && newValue != oldValue)
        {
            if (newValue == true)
            {
                comboBox.Loaded += behavior.OnComboBoxLoaded;
                comboBox.Items.VectorChanged += behavior.Items_VectorChanged;

            }
            else
            {
                comboBox.Loaded -= behavior.OnComboBoxLoaded;
                comboBox.Items.VectorChanged -= behavior.Items_VectorChanged;
            }
        }
    }

    private static void OnAutoMaxWidthPropertyChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        // TODO:
        // TODO: Calculate MaxWidth on loaded
    }

    protected override void OnAttached()
    {
        if (SetWidthFromItems)
        {
            AssociatedObject.Loaded += OnComboBoxLoaded;
            AssociatedObject.Items.VectorChanged += Items_VectorChanged;
        }
    }

    public void Items_VectorChanged(IObservableVector<object> sender, IVectorChangedEventArgs e)
    {
        SetComboBoxWidth(AssociatedObject);
    }

    private void OnComboBoxLoaded(object sender, RoutedEventArgs e)
    {
        SetComboBoxWidth(AssociatedObject);
        container = AssociatedObject.Parent as FrameworkElement;

        if (container is null) return;

        if (container is FrameworkElement fe)
        {
            fe.SizeChanged += Container_SizeChanged;
        }
    }

    private void Container_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Find the total width allowed in the parent of the ComboBox
        if (sender is Grid g)
        {
            int column = (int)AssociatedObject.GetValue(Grid.ColumnProperty);
            int columnSpan = (int)AssociatedObject.GetValue(Grid.ColumnSpanProperty);
            var columns = g.ColumnDefinitions;

            double totalWidth = 0;
            for (int i = column; i < column + columnSpan; i++)
            {
                totalWidth += columns[i].ActualWidth;
            }
            AssociatedObject.MaxWidth = totalWidth;
        }
        else if (sender is FrameworkElement fe)
        {
            AssociatedObject.MaxWidth = fe.ActualWidth;
        }
    }


    /// <summary>
    /// Set the width of a ComboBox to the longest item in its drop down menu
    /// </summary>
    /// <param name="comboBox">Target</param>
    private static void SetComboBoxWidth(ComboBox comboBox)
    {
        // Open ComboBox drop down and prepare ItemContainerGenerator
        comboBox.IsDropDownOpen = true;
        comboBox.ItemContainerGenerator.StartAt(new GeneratorPosition(0, 0), GeneratorDirection.Forward, true);

        double maxWidth = 0;
        ComboBoxItem item;
        while ((item = comboBox.ItemContainerGenerator.GenerateNext(out _) as ComboBoxItem) != null)
        {
            item.Measure(new Windows.Foundation.Size(double.PositiveInfinity, double.PositiveInfinity));
            var size = item.DesiredSize;
            if (size.Width > maxWidth)
            {
                maxWidth = size.Width;
            }
        }

        maxWidth += 20; // This constant adds more space to include the drop down button and paddings
        comboBox.Width = maxWidth > 70 ? maxWidth : 70; // Ensures a MinWidth of 70 when there is no content; Less width will result in issues with appearance
        comboBox.ItemContainerGenerator.Stop();
        comboBox.IsDropDownOpen = false;
    }
}
