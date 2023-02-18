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
using Windows.Foundation;

namespace Natsurainko.FluentLauncher.Behaviors
{
    class SetComboBoxWidthFromItemsBehavior : Behavior<ComboBox>
    {
        public static readonly DependencyProperty SetComboBoxWidthFromItemsProperty =
        DependencyProperty.RegisterAttached
        (
            "SetComboBoxWidthFromItems",
            typeof(bool),
            typeof(SetComboBoxWidthFromItemsBehavior),
            new PropertyMetadata(false, OnSetComboBoxWidthFromItemsPropertyChanged)
        );

        public bool SetComboBoxWidthFromItems 
        { 
            get => (bool)GetValue(SetComboBoxWidthFromItemsProperty); 
            set => SetValue(SetComboBoxWidthFromItemsProperty, value);
        }
        
        protected override void OnAttached()
        {
            AssociatedObject.Loaded += OnComboBoxLoaded;
        }

        private static void OnSetComboBoxWidthFromItemsPropertyChanged(
            DependencyObject dpo, 
            DependencyPropertyChangedEventArgs e)
        {
            ComboBox comboBox = ((SetComboBoxWidthFromItemsBehavior)dpo).AssociatedObject;
            bool newValue = (bool)e.NewValue;
            bool oldValue = (bool)e.OldValue;

            if (comboBox != null && newValue != oldValue)
            {
                if (newValue == true)
                {
                    comboBox.Loaded += OnComboBoxLoaded;
                }
                else
                {
                    comboBox.Loaded -= OnComboBoxLoaded;
                }
            }
        }

        private static void OnComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            ComboBox comboBox = (ComboBox) sender;

            comboBox.DispatcherQueue.TryEnqueue(() => { comboBox.SetWidthFromItems(); });
        }
    }

    public static class ComboBoxExtensionMethods
    {
        public static void SetWidthFromItems(this ComboBox comboBox)
        {
            double comboBoxWidth = 60; // size of combobox without content

            // Create the peer and provider to expand the comboBox in code behind. 
            ComboBoxAutomationPeer peer = new ComboBoxAutomationPeer(comboBox);
            IExpandCollapseProvider provider = (IExpandCollapseProvider)peer.GetPattern(PatternInterface.ExpandCollapse);

            EventHandler<object> eventHandler = null;
            eventHandler = new EventHandler<object>((_, _) =>
            {
                if (comboBox.IsDropDownOpen //&&
                    /*comboBox.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated*/)
                {
                    double width = 0;

                    // Get the container of the item
                    foreach (var item in comboBox.Items)
                    {
                        //TODO: combobox.items are not necessarily ComboBoxItems
                        // var cont = comboBox.ContainerFromIndex(0);
                        // var a = comboBox.ItemContainerGenerator.ContainerFromItem(item);
                        TextBlock comboBoxItem = new TextBlock { Text = item.ToString() };
                        comboBoxItem.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        if (comboBoxItem.DesiredSize.Width > width)
                        {
                            width = comboBoxItem.DesiredSize.Width;
                        }
                    }
                    comboBox.Width = comboBoxWidth + width;
                    // Remove the event handler. 
                    // comboBox.ItemContainerGenerator.StatusChanged -= eventHandler;
                    comboBox.DropDownOpened -= eventHandler;
                    provider.Collapse();
                }
            });

            // comboBox.ItemContainerGenerator.StatusChanged += eventHandler;
            comboBox.DropDownOpened += eventHandler;

            // Expand the comboBox to generate all its ComboBoxItem's. 
            provider.Expand();
        }
    }
}
