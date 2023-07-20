using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Services;
using Natsurainko.FluentLauncher.Services.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Activities;

internal partial class NewsViewModel : ObservableObject
{
    public NewsViewModel(OfficialNewsService service)
    {
        Task.Run(async () =>
        {
            var newsContentDatas = await service.GetOfficialNews();

            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                NewsContentDatas = newsContentDatas;
                Loading = Visibility.Collapsed;
            });
        }).ContinueWith(task =>
        {
            if (task.IsFaulted)
                ;
        });
    }

    [ObservableProperty]
    private IReadOnlyList<NewsContentData> newsContentDatas;

    [ObservableProperty]
    private Visibility loading = Visibility.Visible;
}
