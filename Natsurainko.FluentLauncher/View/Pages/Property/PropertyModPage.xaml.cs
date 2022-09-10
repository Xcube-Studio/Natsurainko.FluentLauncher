using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Class.ViewData;
using Natsurainko.FluentLauncher.Shared.Class.Model;
using Natsurainko.FluentLauncher.Shared.Mapping;
using Natsurainko.FluentLauncher.View.Pages.Resources;
using Natsurainko.FluentLauncher.ViewModel.Pages.Property;
using Richasy.ExpanderEx.Uwp;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Natsurainko.FluentLauncher.View.Pages.Property;

public sealed partial class PropertyModPage : Page
{
    public bool ToggledEnable = false;

    public PropertyModPageVM ViewModel { get; set; }

    public PropertyModPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        var core = (GameCoreViewData)e.Parameter;

        ViewModel = ViewModelBuilder.Build<PropertyModPageVM, PropertyModPage>(this);
        ViewModel.Set(core.Data);
    }

    private void ExpanderEx_PointerEntered(object sender, PointerRoutedEventArgs e)
        => ((Button)((ExpanderEx)sender).FindName("DeleteButton")).Visibility = Visibility.Visible;

    private void ExpanderEx_PointerExited(object sender, PointerRoutedEventArgs e)
        => ((Button)((ExpanderEx)sender).FindName("DeleteButton")).Visibility = Visibility.Collapsed;

    private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        => MainContainer.ContentFrame.Navigate(typeof(ResourcesPage));

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        await LocalModManager.DeleteModOfGameCore((LocalModInformation)((Button)sender).DataContext);
        ViewModel.UpdateMods();
    }

    private async void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        if (!ToggledEnable)
            return;

        var info = (LocalModInformationViewData)((ToggleSwitch)sender).DataContext;

        info.Data.FileInfo = new FileInfo(Path.Combine(info.Data.FileInfo.Directory.FullName, info.Data.FileName + (info.Data.Enable ? ".jar" : ".disabled")));

        await LocalModManager.SwitchModOfGameCore(info.Data);
        info.Data.Enable = !info.Data.Enable;
    }
}
