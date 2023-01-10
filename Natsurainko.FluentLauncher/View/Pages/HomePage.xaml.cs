using Natsurainko.FluentLauncher.Class.AppData;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Class.ViewData;
using Natsurainko.FluentLauncher.View.Pages.Settings;
using Natsurainko.FluentLauncher.ViewModel.Pages;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.View.Pages;

public sealed partial class HomePage : Page
{
    public HomePageVM ViewModel { get; set; }

    public HomePage()
    {
        this.InitializeComponent();

        ViewModel = ViewModelBuilder.Build<HomePageVM, Page>(this);

        Task.Run(async () =>
        {
            if (CacheResources.NewsViewDatas == null)
                await CacheResources.BeginDownloadNews();

            DispatcherHelper.RunAsync(() =>
            {
                ViewModel.NewsViews = CacheResources.NewsViewDatas.Take(5).ToList();
                ViewModel.ShowNews = (Visibility)ConfigurationManager.AppSettings.ShowNews.GetValueOrDefault();
            });
        });
    }

    private void LaunchButton_Click(object sender, RoutedEventArgs e)
        => LauncherProcessViewData.CreateLaunchProcess(ViewModel.CurrentGameCore?.Data, (Button)sender);

    private void AccountButton_Click(object sender, RoutedEventArgs e)
        => this.Frame.Navigate(typeof(SettingsPage), typeof(SettingAccountPage));

    private void NewsButton_Click(object sender, RoutedEventArgs e)
        => this.ViewModel.ShowNews = this.ViewModel.ShowNews == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
}
