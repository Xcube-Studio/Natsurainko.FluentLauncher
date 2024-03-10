using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Activities;

internal partial class NewsViewModel : ObservableObject
{
    // Singleton data
    // TODO: create news service
    private static IReadOnlyList<NewsData> _newsData;
    private static bool _isLoaded = false;

    public NewsViewModel(InterfaceCacheService service)
    {
        if (!_isLoaded)
            LoadNews(service).ContinueWith((_) =>
            {
                App.DispatcherQueue.TryEnqueue(() =>
                {
                    NewsData = _newsData;
                    Loading = _isLoaded ? Visibility.Collapsed : Visibility.Visible;
                });
            });
    }

    private async Task LoadNews(InterfaceCacheService service)
    {
        var newsData = await service.FetchNews().ConfigureAwait(true);
        _newsData = newsData;
        _isLoaded = true;
    }

    [ObservableProperty]
    private IReadOnlyList<NewsData> newsData;

    [ObservableProperty]
    private Visibility loading;
}
