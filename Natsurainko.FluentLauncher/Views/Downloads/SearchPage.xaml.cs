using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Downloads;
using System;

namespace Natsurainko.FluentLauncher.Views.Downloads;

public sealed partial class SearchPage : Page, IBreadcrumbBarAware
{
    public string Route => "Search";

    SearchViewModel VM => (SearchViewModel)DataContext;

    public SearchPage()
    {
        InitializeComponent();
    }

    private void Page_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        GC.Collect();
    }
}
