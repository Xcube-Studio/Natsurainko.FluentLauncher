using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentLauncher.Classes.Data.UI;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Views.Downloads;
using Natsurainko.FluentLauncher.Views;
using Nrk.FluentCore.Classes.Datas.Download;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Classes.Data.Download;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads;

internal partial class DownloadsViewModel : ObservableObject
{
    private readonly InterfaceCacheService _interfaceCacheService = App.GetService<InterfaceCacheService>();

    public DownloadsViewModel()
    {
        Task.Run(async () =>
        {
            var publishDatas = await _interfaceCacheService.FetchMinecraftPublishes();

            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                PrimaryPublishData = publishDatas[0];
                SecondaryPublishData = publishDatas[1];
            });
        });

        Task.Run(() =>
        {
            _interfaceCacheService.FetchCurseForgeFeaturedResources(out var McMods, out var ModPacks);

            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                CurseMcMods = McMods;
                CurseModPacks = ModPacks;
            });
        });
    }

    [ObservableProperty]
    private string searchBoxInput = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ModSearchProperty))]
    private int resourceType;

    [ObservableProperty]
    private PublishData primaryPublishData;

    [ObservableProperty]
    private PublishData secondaryPublishData;

    [ObservableProperty]
    private IEnumerable<CurseResource> curseMcMods;

    [ObservableProperty]
    private IEnumerable<CurseResource> curseModPacks;

    [ObservableProperty]
    private IEnumerable<string> versions;

    [ObservableProperty]
    private string selectedVersion;

    [ObservableProperty]
    private int selectedSource;

    public Visibility ModSearchProperty => ResourceType == 0 ? Visibility.Collapsed : Visibility.Visible;

    [RelayCommand]
    public void NavigateCurseResourcePage(CurseResource curseResource)
        => ShellPage.ContentFrame.Navigate(typeof(CurseResourcePage), curseResource);

    [RelayCommand]
    public void SearchAllMinecraft()
        => ShellPage.ContentFrame.Navigate(typeof(ResourcesSearchPage), new ResourceSearchData
        {
            SearchInput = string.Empty,
            ResourceType = 0,
            Source = SelectedSource,
            Version = SelectedVersion,
        });

    [RelayCommand]
    public void Search()
        => ShellPage.ContentFrame.Navigate(typeof(ResourcesSearchPage), new ResourceSearchData
        {
            SearchInput = SearchBoxInput,
            ResourceType = ResourceType,
            Source = SelectedSource,
            Version = SelectedVersion,
        });
}
