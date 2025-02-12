using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.ViewModels.Downloads.Mods;
using Natsurainko.FluentLauncher.XamlHelpers.Converters;
using Nrk.FluentCore.Resources;

namespace Natsurainko.FluentLauncher.Views.Downloads.Mods;

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
            if (contentFrame.Content.GetType() == typeof(ModPage))
            {
                string modName = string.Empty;

                if (e.Parameter is CurseForgeResource curseForgeResource)
                    modName = curseForgeResource.Name.Replace('/', ' ');
                else if (e.Parameter is ModrinthResource modrinthResource)
                    modName = modrinthResource.Name.Replace('/', ' ');

                var converter = (BreadcrumbBarLocalizationConverter)breadcrumbBar.Resources["BreadcrumbBarLocalizationConverter"];
                if (!converter.IgnoredText.Contains(modName))
                    converter.IgnoredText.Add(modName);

                breadcrumbBar.SetPath($"ModsDownload/{modName}");
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
