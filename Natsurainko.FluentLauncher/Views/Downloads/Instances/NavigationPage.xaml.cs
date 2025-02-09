using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.ViewModels.Downloads.Instances;
using Natsurainko.FluentLauncher.XamlHelpers.Converters;
using Nrk.FluentCore.GameManagement.Installer;

namespace Natsurainko.FluentLauncher.Views.Downloads.Instances;

public sealed partial class NavigationPage : Page, INavigationProvider
{
    object INavigationProvider.NavigationControl => contentFrame;
    NavigationViewModel VM => (NavigationViewModel)DataContext;
    INavigationService INavigationProvider.NavigationService => VM.NavigationService;

    public NavigationPage()
    {
        this.InitializeComponent();
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        var breadcrumbBarAware = (IBreadcrumbBarAware)(contentFrame.Content);

        if (e.NavigationMode == NavigationMode.Back)
            breadcrumbBar.GoBack();
        else
        {
            if (contentFrame.Content.GetType() == typeof(InstallPage))
            {
                var instance = (VersionManifestItem)e.Parameter;
                VM.CurrentInstance = instance;
                string instanceId = instance.Id;

                var converter = (BreadcrumbBarLocalizationConverter)breadcrumbBar.Resources["BreadcrumbBarLocalizationConverter"];
                if (!converter.IgnoredText.Contains(instanceId))
                    converter.IgnoredText.Add(instanceId);

                breadcrumbBar.SetPath($"InstancesDownload/{instanceId}");
            }
            else
            {
                breadcrumbBar.AddItem(breadcrumbBarAware.Route);
            }
        }
    }

    private void breadcrumbBar_ItemClicked(object sender, string[] args) => VM.HandleNavigationBreadcrumBarItemClicked(args);

    private void Page_Loaded(object sender, RoutedEventArgs e) => breadcrumbBar.Items = VM.DisplayedPath;
}
