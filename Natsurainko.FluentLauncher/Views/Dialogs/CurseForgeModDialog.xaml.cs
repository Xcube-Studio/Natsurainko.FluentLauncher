using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Views.Downloads;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Natsurainko.FluentLauncher.Views.Dialogs;

public sealed partial class CurseForgeModDialog : ContentDialog
{
    public CurseForgeModDialog()
    {
        this.InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e) => Hide();

    private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        => contentFrame.Navigate(Type.GetType(((NavigationViewItem)args.InvokedItemContainer).Tag.ToString()), this.DataContext);

    private void Dialog_Loaded(object sender, RoutedEventArgs e)
        => contentFrame.Navigate(typeof(CurseForgeModInfo), this.DataContext);
}
