using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using System;

namespace Natsurainko.FluentLauncher.Utils.Xaml.Behaviors
{
    class SettingsCardContentMaxWidthBehavior : Behavior<FrameworkElement>
    {
        #region AutoMaxWidthProperty

        public static readonly DependencyProperty AutoMaxWidthProperty =
            DependencyProperty.RegisterAttached
            (
                nameof(AutoMaxWidthProperty),
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

        #region AscendentTypeProperty

        public static readonly DependencyProperty AscendentTypeProperty =
            DependencyProperty.RegisterAttached
            (
                nameof(AscendentTypeProperty),
                typeof(Type),
                typeof(SettingsCardContentMaxWidthBehavior),
                new PropertyMetadata(null)
            );

        public Type AscendentType
        {
            get => (Type)GetValue(AscendentTypeProperty);
            set => SetValue(AscendentTypeProperty, value);
        }

        #endregion

        FrameworkElement container = null;

        FrameworkElement target = null;

        private static void OnAutoMaxWidthPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            SettingsCardContentMaxWidthBehavior behavior = (SettingsCardContentMaxWidthBehavior)d;

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
            target = AssociatedObject;

            if (AutoMaxWidth)
            {
                AssociatedObject.Loaded += FrameworkElement_Loaded;
            }
        }

        private void FrameworkElement_Loaded(object sender, RoutedEventArgs e)
        {
            if (AscendentType is null)
                container = target.Parent as FrameworkElement;
            else
                container = target.FindAscendant(AscendentType) as FrameworkElement;

            if (container is null) return;

            container.SizeChanged += Container_SizeChanged;
            SetMaxWidth(container, target);
        }

        private void Container_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetMaxWidth(sender as FrameworkElement, target);
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
                var ascs = target.FindAscendants();
                var enumerator = ascs.GetEnumerator();

                // Find the child of the Grid `g` which holds `target`
                DependencyObject current = target;
                DependencyObject targetContainer = null;
                while (current != g && enumerator.MoveNext())
                {
                    targetContainer = current; // `targetContainer` is always the item before `current` in the enumerator
                    current = enumerator.Current;
                }

                // Find if the SettingsCard is wrapped for small width
                bool isWrapped = (int)targetContainer.GetValue(Grid.RowProperty) == 1;

                // Find the appropriate column width
                double totalWidth = g.ActualWidth;

                // Subtract icon column width
                var icon = g.FindDescendant("PART_HeaderIconPresenterHolder") as FrameworkElement;
                var iconColumn = g.ColumnDefinitions[0];
                var iconWidth = icon is null ? 0 : icon.ActualWidth;
                totalWidth -= iconWidth > iconColumn.ActualWidth ? iconWidth : iconColumn.ActualWidth;

                //var action = g.FindDescendant("PART_ActionIconPresenter") as FrameworkElement;
                //totalWidth -= action is null ? 0 : action.ActualWidth;

                // Subtract indentation (including action icon) width
                if (isWrapped)
                    totalWidth = g.ActualWidth - 150;
                else
                    totalWidth = g.ActualWidth - 190;

                // Subtract header width
                if (!isWrapped)
                {
                    var header = g.FindDescendant("PART_HeaderPresenter") as ContentPresenter;
                    if (header is not null)
                    {
                        var content = header.FindDescendant<FrameworkElement>();
                        if (content is not null)
                            totalWidth -= content.ActualWidth;
                        else
                            totalWidth -= header.ActualWidth;
                    }
                }

                target.MaxWidth = Math.Floor(totalWidth);
            }
            else
            {
                target.MaxWidth = container.ActualWidth;
            }
        }

    }
}
