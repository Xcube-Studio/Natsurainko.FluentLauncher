using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Nrk.FluentCore.Launch;
using System.Collections.ObjectModel;

namespace Natsurainko.FluentLauncher.ViewModels.Tasks;

class LaunchSessions : ObservableObject
{
    public ObservableCollection<LaunchSessionViewModel> SessionViewModels = new();

    public LaunchSessions(LaunchService launchService)
    {
        launchService.SessionCreated += LaunchService_SessionCreated;
    }

    private void LaunchService_SessionCreated(object? sender, MinecraftSession e)
    {
        var launchSessionViewModel = new LaunchSessionViewModel(e);
        App.DispatcherQueue.TryEnqueue(() => SessionViewModels.Insert(0, launchSessionViewModel));
    }
}
