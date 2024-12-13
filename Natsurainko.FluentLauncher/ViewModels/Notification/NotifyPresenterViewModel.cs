using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Natsurainko.FluentLauncher.Utils.Extensions;
using System;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Notification;

internal partial class NotifyPresenterViewModel : ObservableObject
{
    public bool _removed = false;

    [ObservableProperty]
    public partial string NotifyTitile { get; set; }

    [ObservableProperty]
    public partial string Icon { get; set; }

    [ObservableProperty]
    public partial ContentPresenter NotifyContent { get; set; }
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
