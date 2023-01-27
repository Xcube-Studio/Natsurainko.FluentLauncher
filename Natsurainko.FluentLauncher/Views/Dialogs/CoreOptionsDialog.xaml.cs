using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Views.Pages.Properties;
using System;

namespace Natsurainko.FluentLauncher.Views.Dialogs;

public sealed partial class CoreOptionsDialog : ContentDialog
{
    public CoreOptionsDialog()
    {
        this.InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e) => Hide();

    private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        => contentFrame.Navigate(Type.GetType(((NavigationViewItem)args.InvokedItemContainer).Tag.ToString()), this.DataContext);

    private void Dialog_Loaded(object sender, RoutedEventArgs e)
        => contentFrame.Navigate(typeof(Information), this.DataContext);
}
