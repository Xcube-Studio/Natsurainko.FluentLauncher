using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.GameManagement.Instances;
using Nrk.FluentCore.GameManagement.Saves;
using System.Collections.ObjectModel;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Instances;

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
    void OpenSavesFolder() => ExplorerHelper.OpenFolder(SavesFolder);

    [RelayCommand]
    void OpenSaveFolder(SaveInfo saveInfo) => ExplorerHelper.OpenFolder(saveInfo.Folder);
}
