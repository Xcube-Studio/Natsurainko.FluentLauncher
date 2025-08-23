using FluentLauncher.Infra.UI.Navigation;
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

    internal static string FormatDate(string date) => DateTime.Parse(date).ToString(CultureInfo.CurrentCulture.DateTimeFormat);
}