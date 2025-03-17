using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Experimental.Saves;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.GameManagement.Instances;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.System;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Cores;

internal partial class SaveViewModel : PageVM, INavigationAware
{
    public MinecraftInstance MinecraftInstance { get; private set; }

    public string SavesFolder { get; private set; }

    public ObservableCollection<SaveInfo> Saves { get; private set; } = [];

    async void INavigationAware.OnNavigatedTo(object parameter)
    {
        MinecraftInstance = parameter as MinecraftInstance;
        SavesFolder = MinecraftInstance.GetSavesDirectory();

        var manager = new SaveManager(SavesFolder);

        await foreach (var saveInfo in manager.EnumerateSavesAsync())
            await Dispatcher.EnqueueAsync(() => Saves.Add(saveInfo));
    }

    [RelayCommand]
    async Task OpenSavesFolder() => await Launcher.LaunchFolderPathAsync(SavesFolder);

    [RelayCommand]
    async Task OpenSaveFolder(SaveInfo saveInfo) => await Launcher.LaunchFolderPathAsync(saveInfo.Folder);
}
