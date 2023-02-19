using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Behaviors
{
    class AutoMaxWidthBehavior : Behavior<FrameworkElement>
    {
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

        private static void OnAutoMaxWidthPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            AutoMaxWidthBehavior behavior = (AutoMaxWidthBehavior)d;

            bool newValue = (bool)e.NewValue;
            bool oldValue = (bool)e.OldValue;

            if (behavior.AssociatedObject is not null && newValue != oldValue)
            {
                if (newValue == true)
                {
                    behavior.AssociatedObject.Loaded += behavior.FrameworkElement_Loaded;
                    if (behavior.container is not null)
                    {
                        behavior.container.SizeChanged += behavior.Container_SizeChanged;
                    }
                }
                else
                {
                    behavior.AssociatedObject.Loaded -= behavior.FrameworkElement_Loaded;
                    if (behavior.container is not null)
                    {
                        behavior.container.SizeChanged -= behavior.Container_SizeChanged;
                    }
                }
            }
        }

        protected override void OnAttached()
        {
            if (AutoMaxWidth)
            {
                AssociatedObject.Loaded += FrameworkElement_Loaded;
            }
        }

        private void FrameworkElement_Loaded(object sender, RoutedEventArgs e)
        {
            container = AssociatedObject.Parent as FrameworkElement;

            if (container is null) return;

            container.SizeChanged += Container_SizeChanged;
            SetMaxWidth(container, AssociatedObject);
        }

        private void Container_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetMaxWidth(sender as FrameworkElement, AssociatedObject);
        }

        /// <summary>
        /// Set the MaxWidth of a FrameworkElement based on its parent
        /// </summary>
        /// <param name="container"></param>
        /// <param name="target"></param>
        public static void SetMaxWidth(FrameworkElement container, FrameworkElement target)
        {
            if (container is null || target is null) return;

            if (container is Grid g)
            {
                int column = (int)target.GetValue(Grid.ColumnProperty);
                int columnSpan = (int)target.GetValue(Grid.ColumnSpanProperty);
                var columns = g.ColumnDefinitions;

                double totalWidth = 0;
                for (int i = column; i < column + columnSpan; i++)
                {
                    totalWidth += columns[i].ActualWidth;
                }
                target.MaxWidth = totalWidth;
            }
            else
            {
                target.MaxWidth = container.ActualWidth;
            }
        }

    }
}
