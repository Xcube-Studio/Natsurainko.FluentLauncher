using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Natsurainko.FluentLauncher.Views.Downloads;

public sealed partial class SearchPage : Page, IBreadcrumbBarAware
{
    public string Route => "Search";

    public SearchPage()
    {
        this.InitializeComponent();
    }

    private void Page_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        this.DataContext = null;
        GC.Collect();
    }
}
