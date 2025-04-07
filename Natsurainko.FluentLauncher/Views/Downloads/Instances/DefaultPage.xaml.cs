using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Downloads.Instances;
using System;
using System.Globalization;

namespace Natsurainko.FluentLauncher.Views.Downloads.Instances;

public sealed partial class DefaultPage : Page, IBreadcrumbBarAware
{
    string IBreadcrumbBarAware.Route => "InstancesDownload";

    DefaultViewModel VM => (DefaultViewModel)DataContext;

    public DefaultPage()
    {
        this.InitializeComponent(); 
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        Update();
        SizeChanged += (_, _) => Update();
    }

    internal static string FormatDate(string date) => DateTime.Parse(date).ToString(CultureInfo.CurrentCulture.DateTimeFormat);

    void Update()
    {
        if (CD1 == null)
            return;

        if (App.MainWindow.Width <= 641)
        {
            CD2.Width = GridLength.Auto;
            CD1.Width = new GridLength(0, GridUnitType.Pixel);
            RD1.Height = new GridLength(4, GridUnitType.Pixel);

            Grid.SetRow(LatestSnapshotPanel, 2);
            Grid.SetColumn(LatestSnapshotPanel, 0);
        }
        else
        {
            CD2.Width = new GridLength(1, GridUnitType.Star);
            CD1.Width = new GridLength(4, GridUnitType.Pixel);
            RD1.Height = new GridLength(0, GridUnitType.Pixel);

            Grid.SetRow(LatestSnapshotPanel, 0);
            Grid.SetColumn(LatestSnapshotPanel, 2);
        }
    }
}