using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Downloads;
using System;

namespace Natsurainko.FluentLauncher.Views.Downloads;

public sealed partial class DefaultPage : Page, IBreadcrumbBarAware
{
    public string Route => "Download";

    DefaultViewModel VM => (DefaultViewModel)DataContext;

    public DefaultPage()
    {
        InitializeComponent();
    }

    private void Page_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        GC.Collect();
    }
}
