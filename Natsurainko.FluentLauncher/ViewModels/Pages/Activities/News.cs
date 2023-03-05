using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Services;
using Natsurainko.FluentLauncher.Services.Data;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Pages.Activities;

internal partial class News : ObservableObject
{
    public News(OfficialNewsService service)
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
