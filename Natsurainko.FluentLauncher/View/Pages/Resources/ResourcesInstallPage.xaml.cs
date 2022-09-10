using Natsurainko.FluentCore.Class.Model.Install;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Class.ViewData;
using Natsurainko.FluentLauncher.ViewModel.Pages.Resources;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.View.Pages.Resources;

public sealed partial class ResourcesInstallPage : Page
{
    public ResourcesInstallPageVM ViewModel { get; set; }

    public ResourcesInstallPage()
    {
        this.InitializeComponent();

        ViewModel = ViewModelBuilder.Build<ResourcesInstallPageVM, Page>(this);
    }

    private void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(ConfigurationManager.AppSettings.CurrentGameFolder))
        {
            MainContainer.ShowInfoBarAsync("未选择游戏目录，无法安装");
            return;
        }

        if (string.IsNullOrEmpty(ConfigurationManager.AppSettings.CurrentJavaRuntime))
        {
            MainContainer.ShowInfoBarAsync("未选择 Java 运行时，无法安装");
            return;
        }

        ModLoaderType? modLoaderType = ViewModel.SelectedModLoader?.Data.LoaderType;
        object installBuild = ViewModel.EnableLoader ? ViewModel.SelectedModLoader.Build : ViewModel.CoreManifestItem.Data;

        MinecraftInstallerViewData.CreateMinecraftInstallProcess
        (
            installBuild,
            $"{ViewModel.CoreManifestItem.Data.Id}{(ViewModel.EnableLoader ? $", {ViewModel.SelectedModLoader.Data.LoaderName}-{ViewModel.SelectedModLoader.Data.Version}" : string.Empty)}",
            sender as Control,
            modLoaderType
        );

        this.Frame.Navigate(typeof(ResourcesPage));
    }
}
