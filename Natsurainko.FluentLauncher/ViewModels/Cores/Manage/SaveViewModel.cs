using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Experimental.Saves;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.Management;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.System;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Cores.Manage;

internal partial class SaveViewModel : ObservableObject, INavigationAware
{
    public GameInfo GameInfo { get; private set; }

    public string SavesFolder { get; private set; }

    public ObservableCollection<SaveInfo> Saves { get; private set; } = [];

    async void INavigationAware.OnNavigatedTo(object parameter)
    {
        GameInfo = parameter as GameInfo;
        SavesFolder = GameInfo.GetSavesDirectory();

        var manager = new SaveManager(SavesFolder);

        await foreach (var saveInfo in manager.EnumerateSavesAsync())
            App.DispatcherQueue.TryEnqueue(() => Saves.Add(saveInfo));
    }

    [RelayCommand]
    public async Task OpenSavesFolder() => await Launcher.LaunchFolderPathAsync(SavesFolder);

    [RelayCommand]
    public async Task OpenSaveFolder(SaveInfo saveInfo) => await Launcher.LaunchFolderPathAsync(saveInfo.Folder);
}
