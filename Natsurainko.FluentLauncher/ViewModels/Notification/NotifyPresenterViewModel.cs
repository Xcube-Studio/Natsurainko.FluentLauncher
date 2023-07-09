using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Natsurainko.FluentLauncher.Utils.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Notification;

internal partial class NotifyPresenterViewModel : ObservableObject
{
    [ObservableProperty]
    private string notifyTitile;

    [ObservableProperty]
    private ContentPresenter notifyContent;

    public Storyboard RetractAnimation { get; set; }

    public Action Remove { get; set; }

    [RelayCommand]
    public Task Close() => Task.Run(() =>
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
        {
            await RetractAnimation.BeginAsync();
            Remove();
        });
    });
}
