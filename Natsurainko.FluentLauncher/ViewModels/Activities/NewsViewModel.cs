using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Classes.Data.UI;
using Natsurainko.FluentLauncher.Services.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Activities;

internal partial class NewsViewModel : ObservableObject
{
    public NewsViewModel(InterfaceCacheService service)
    {
        Task.Run(async () =>
        {
            var newsDatas = await service.FetchNews();

            App.DispatcherQueue.TryEnqueue(() =>
            {
                NewsDatas = newsDatas;
                Loading = Visibility.Collapsed;
            });
        }).ContinueWith(task =>
        {
            if (task.IsFaulted)
                ;
        });
    }

    [ObservableProperty]
    private IReadOnlyList<NewsData> newsDatas;

    [ObservableProperty]
    private Visibility loading = Visibility.Visible;
}
