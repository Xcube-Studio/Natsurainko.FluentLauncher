using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Natsurainko.FluentLauncher.Utils.Xaml;
using System;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Notification;

internal partial class NotifyPresenterViewModel : ObservableObject
{
    public bool _removed = false;

    [ObservableProperty]
    private string notifyTitile;

    [ObservableProperty]
    private string icon;

    [ObservableProperty]
    private ContentPresenter notifyContent;

    public Func<Storyboard> CreateRetractAnimationAction { get; set; }

    public Action Remove { get; set; }

    [RelayCommand]
    public Task Close() => Task.Run(() =>
    {
        App.DispatcherQueue.TryEnqueue(async () =>
        {
            var retractAnimation = CreateRetractAnimationAction();
            await retractAnimation.BeginAsync();
            Remove();

            _removed = true;
        });
    });
}
