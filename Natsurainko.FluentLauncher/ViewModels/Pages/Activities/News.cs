using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Pages.Activities;

public partial class News : ObservableObject
{
    public News() 
    {
        Loading = Visibility.Visible;

        Task.Run(async () =>
        {
            try
            {
                if (GlobalActivitiesCache.MojangNews == null)
                    await GlobalActivitiesCache.BeginDownloadNews();
            }
            catch { }

            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                NewsModels = GlobalActivitiesCache.MojangNews;
                Loading = Visibility.Collapsed;
            });
        });
    }

    [ObservableProperty]
    public List<NewsData> newsModels;

    [ObservableProperty]
    public Visibility loading;
}
