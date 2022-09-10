using Natsurainko.FluentLauncher.View.Dialogs;
using Richasy.ExpanderEx.Uwp;
using System;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.View.Pages.Resources;

public sealed partial class ResourcesPage : Page
{
    public ResourcesPage()
    {
        this.InitializeComponent();
    }

    private void ExpanderEx_Click(object sender, ExpanderExClickEventArgs e)
    {
        if (((ExpanderEx)sender).Tag != null)
            this.Frame.Navigate(Type.GetType($"Natsurainko.FluentLauncher.View.Pages.Resources.{((ExpanderEx)sender).Tag}"));
    }

    private async void JavaExpanderEx_Click(object sender, ExpanderExClickEventArgs e)
        => await new InstallJavaDialog().ShowAsync();
}
