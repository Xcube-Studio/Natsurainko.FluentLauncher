using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace Natsurainko.FluentLauncher.UserControls;

public sealed partial class ExpanderContentItem : UserControl
{
    public static readonly DependencyProperty PresentContentProperty =
        DependencyProperty.Register("PresentContent", typeof(FrameworkElement), typeof(ExpanderContentItem), new PropertyMetadata(null));

    public ExpanderContentItem()
    {
        this.InitializeComponent();

        this.SizeChanged += this.OnSizeChanged;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (this.Parent != null && this.Content != null)
        {
            this.Padding = (this.Parent as FrameworkElement).ActualWidth < 500d
                ? new Thickness(15, 0, 15, 0)
                : new Thickness(50, 0, 60, 0);

            (this.Content as FrameworkElement).Margin = this.Padding;
        }
    }
}
