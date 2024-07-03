using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.OOBE;
using System.Linq;

namespace Natsurainko.FluentLauncher.Views.OOBE;

public sealed partial class OOBENavigationPage : Page, INavigationProvider
{
    object INavigationProvider.NavigationControl => contentFrame;
    private OOBEViewModel VM => (OOBEViewModel)DataContext;

    public NavigationTransitionInfo TransitionInfo
    {
        get => navTransition.DefaultNavigationTransitionInfo;
        set => navTransition.DefaultNavigationTransitionInfo = value;
    }

    public OOBENavigationPage()
    {
        InitializeComponent();
    }

    // Explicitly set transition effect at each navigation
    bool bypassTransitionUpdate = false; // Bypass transition update if NavigationViewItem is updated in ContentFrame_Navigated
    private void NavigationViewControl_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (bypassTransitionUpdate)
            return;

        int sourcePageIndex = VM.CurrentPageIndex;

        var navigationViewItems = sender.MenuItems.Union(sender.FooterMenuItems).Cast<NavigationViewItem>().Select(item => item.Tag).Cast<string>().ToList();
        string pageTag = ((NavigationViewItem)args.InvokedItemContainer).GetTag();
        int targetPageIndex = navigationViewItems.IndexOf(pageTag);

        // Set transition direction
        if (targetPageIndex > sourcePageIndex)
            TransitionInfo = new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight };
        else if (targetPageIndex < sourcePageIndex)
            TransitionInfo = new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromLeft };
        else
            TransitionInfo = new EntranceNavigationTransitionInfo();

        VM.NavigateTo(targetPageIndex);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        TransitionInfo = new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromLeft };
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        TransitionInfo = new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight };
    }

    // Update NavigationViewItem selection
    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        foreach (NavigationViewItem item in NavigationView.MenuItems.Union(NavigationView.FooterMenuItems).Cast<NavigationViewItem>())
        {
            if (App.GetService<IPageProvider>().RegisteredPages[item.GetTag()].PageType == e.SourcePageType)
            {
                bypassTransitionUpdate = true;
                NavigationView.SelectedItem = item;
                bypassTransitionUpdate = false;

                item.IsSelected = true;
                item.IsEnabled = true;
                return;
            }
        }
    }
}
