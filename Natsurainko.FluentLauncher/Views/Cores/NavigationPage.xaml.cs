using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels.Cores;
using Natsurainko.FluentLauncher.XamlHelpers.Converters;
using Nrk.FluentCore.GameManagement.Instances;

namespace Natsurainko.FluentLauncher.Views.Cores;

public sealed partial class NavigationPage : Page, INavigationProvider
{
    object INavigationProvider.NavigationControl => contentFrame;
    INavigationService INavigationProvider.NavigationService => VM.NavigationService;

    NavigationViewModel VM => (NavigationViewModel)DataContext;

    public NavigationPage()
    {
        InitializeComponent();
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        var breadcrumbBarAware = (IBreadcrumbBarAware)(contentFrame.Content);
        if (e.NavigationMode == NavigationMode.Back)
        {
            breadcrumbBar.GoBack();
        }
        else
        {
            if (contentFrame.Content.GetType() == typeof(InstancePage))
            {
                var instance = (MinecraftInstance)e.Parameter;
                VM.CurrentInstance = instance;
                string instanceId = instance.InstanceId;

                var converter = (BreadcrumbBarLocalizationConverter)breadcrumbBar.Resources["BreadcrumbBarLocalizationConverter"];
                if (!converter.IgnoredText.Contains(instanceId))
                    converter.IgnoredText.Add(instanceId);

                breadcrumbBar.SetPath($"Cores/{instance.InstanceId}");
            }
            else
            {
                breadcrumbBar.AddItem(breadcrumbBarAware.Route);
            }
        }
    }

    private void breadcrumbBar_ItemClicked(object sender, string[] args)
    {
        VM.HandleNavigationBreadcrumBarItemClicked(args);
    }

    //private void breadcrumbBar_Loading(Microsoft.UI.Xaml.FrameworkElement sender, object args)
    //{
    //    VM.HandleBreadcrumbBarLoading(breadcrumbBar.Resources["BreadcrumbBarLocalizationConverter"]);
    //}

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        breadcrumbBar.Items = VM.DisplayedPath;
    }
}
