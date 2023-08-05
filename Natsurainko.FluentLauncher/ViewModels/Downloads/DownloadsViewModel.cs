using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Classes.Data.UI;
using Natsurainko.FluentLauncher.Services.Storage;
using System;
using System.Threading.Tasks;

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
    }

    [ObservableProperty]
    private PublishData primaryPublishData;

    [ObservableProperty]
    private PublishData secondaryPublishData;
}
