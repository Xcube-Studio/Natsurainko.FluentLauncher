using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.GameManagement.Instances;
using Nrk.FluentCore.GameManagement.Mods;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.System;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Cores;

public partial class ModViewModel : ObservableObject, INavigationAware
{
    public ModManager ModsManager { get; private set; }

    public string ModsFolder { get; private set; }

    public MinecraftInstance MinecraftInstance { get; private set; }

    public bool NotSupportMod => !MinecraftInstance.IsSupportMod();

    public ObservableCollection<MinecraftMod> Mods { get; private set; } = [];

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        MinecraftInstance = parameter as MinecraftInstance;
        ModsFolder = MinecraftInstance.GetModsDirectory();

        if (!Directory.Exists(ModsFolder))
            Directory.CreateDirectory(ModsFolder);

        ModsManager = new ModManager(ModsFolder);

        LoadModList();
    }

    [RelayCommand]
    async Task OpenModsFolder() => await Launcher.LaunchFolderPathAsync(ModsFolder);

    [RelayCommand]
    void DeleteMod(MinecraftMod modInfo)
    {
        File.Delete(modInfo.AbsolutePath);
        LoadModList();
    }

    [RelayCommand]
    void InstallMods() => WeakReferenceMessenger.Default.Send(new GlobalNavigationMessage("ModsDownload/Navigation"));

    async void LoadModList()
    {
        Mods.Clear();

        try
        {
            await foreach (var saveInfo in ModsManager.EnumerateModsAsync())
                await App.DispatcherQueue.EnqueueAsync(() => Mods.Add(saveInfo));
        }
        catch { }
    }
}
