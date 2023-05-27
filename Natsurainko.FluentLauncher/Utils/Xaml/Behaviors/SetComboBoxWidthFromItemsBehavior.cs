using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.Xaml.Interactivity;
using Windows.Foundation.Collections;

namespace Natsurainko.FluentLauncher.Utils.Xaml.Behaviors;

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

    ComboBox target;

    private static void OnSetWidthFromItemsPropertyChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        SetComboBoxWidthFromItemsBehavior behavior = (SetComboBoxWidthFromItemsBehavior)d;
        ComboBox comboBox = behavior.AssociatedObject;

        bool newValue = (bool)e.NewValue;
        bool oldValue = (bool)e.OldValue;

        if (comboBox is not null && newValue != oldValue)
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

    protected override void OnAttached()
    {
        // The associated object becomes null under some conditions.
        // The reference to the true associated object is held privately when initialized.
        target = AssociatedObject;

        if (SetWidthFromItems)
        {
            AssociatedObject.Loaded += OnComboBoxLoaded;
            AssociatedObject.Items.VectorChanged += Items_VectorChanged;
        }
    }

    public void Items_VectorChanged(IObservableVector<object> sender, IVectorChangedEventArgs e)
    {
        SetComboBoxWidth(target);
    }

    private void OnComboBoxLoaded(object sender, RoutedEventArgs e)
    {
        SetComboBoxWidth(target);
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
