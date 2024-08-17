using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Nrk.FluentCore.Launch;
using System;
using System.Collections.ObjectModel;

namespace Natsurainko.FluentLauncher.ViewModels.Tasks;

class LaunchSessions : ObservableObject
{
    public ObservableCollection<LaunchSessionViewModel> SessionViewModels = new();

    public LaunchSessionViewModel CreateLaunchSessionViewModel(MinecraftSession session, out Action<Exception> handleException)
    {
        var launchSessionViewModel = new LaunchSessionViewModel(session);
        App.DispatcherQueue.TryEnqueue(() => SessionViewModels.Insert(0, launchSessionViewModel));

        handleException = launchSessionViewModel.OnExceptionThrow;
        return launchSessionViewModel;
    }
}
