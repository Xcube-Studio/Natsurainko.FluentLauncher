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
        if (e.NavigationMode == NavigationMode.Back)
            breadcrumbBar.GoBack();
        else
        {
            if (contentFrame.Content is ResourcePage)
            {
                string resourceName = string.Empty;

                if (e.Parameter is CurseForgeResource curseForgeResource)
                    resourceName = curseForgeResource.Name.Replace('/', ' ');
                else if (e.Parameter is ModrinthResource modrinthResource)
                    resourceName = modrinthResource.Name.Replace('/', ' ');

                var converter = (BreadcrumbBarLocalizationConverter)breadcrumbBar.Resources["BreadcrumbBarLocalizationConverter"];
                if (!converter.IgnoredText.Contains(resourceName))
                    converter.IgnoredText.Add(resourceName);

                breadcrumbBar.SetPath($"ModsDownload/{resourceName}");
            }
            else if (contentFrame.Content is ResourceDefaultPage)
            {
                breadcrumbBar.AddItem("ModsDownload");
            }
        }
    }

    private void breadcrumbBar_ItemClicked(object sender, string[] args) => VM.HandleNavigationBreadcrumBarItemClicked(args);

    private void Page_Loaded(object sender, RoutedEventArgs e) => breadcrumbBar.Items = VM.DisplayedPath;
}
