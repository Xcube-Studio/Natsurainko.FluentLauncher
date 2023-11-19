using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Components.Launch;
using Natsurainko.FluentLauncher.Services.Launch;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Activities;

class LaunchSessions : ObservableObject
{
    public ObservableCollection<LaunchSessionViewModel> SessionViewModels = new();

    public LaunchSessions(LaunchService launchService)
    {
        launchService.SessionCreated += LaunchService_SessionCreated;
    }

    private void LaunchService_SessionCreated(object sender, MinecraftSession e)
    {
        App.DispatcherQueue.TryEnqueue(() =>
        {
            SessionViewModels.Insert(0, new LaunchSessionViewModel(e));
        });
    }
}
