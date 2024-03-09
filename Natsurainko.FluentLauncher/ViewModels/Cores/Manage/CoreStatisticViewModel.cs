using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using Nrk.FluentCore.Management;
using Nrk.FluentCore.Utils;
using System;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Cores.Manage;

internal partial class CoreStatisticViewModel : ObservableObject, INavigationAware
{
    private GameInfo _gameInfo;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormatSize))]
    private GameStatisticInfo gameStatisticInfo;

    public string FormatSize
    {
        get
        {
            if (GameStatisticInfo == null)
                return string.Empty;

            double d = GameStatisticInfo.TotalSize;
            int i = 0;

            while ((d > 1024) && (i < 5))
            {
                d /= 1024;
                i++;
            }

            var unit = new string[] { "B", "KB", "MB", "GB", "TB" };
            return string.Format("{0} {1}", Math.Round(d, 2), unit[i]);
        }
    }

    public bool IsVanilla => _gameInfo.IsVanilla;

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        if (parameter is not GameInfo gameInfo)
            throw new ArgumentException("Invalid parameter type");

        _gameInfo = gameInfo;

        Task.Run(() =>
        {
            var info = _gameInfo.GetStatisticInfo();
            App.DispatcherQueue.TryEnqueue(() => GameStatisticInfo = info);
        });
    }
}
