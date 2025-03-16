using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using FluentLauncher.Infra.UI.Navigation;
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

internal partial class ModViewModel : PageVM, INavigationAware
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
    void InstallMods() => GlobalNavigate("ModsDownload/Navigation");

    async void LoadModList()
    {
        Mods.Clear();

        try
        {
            await foreach (var saveInfo in ModsManager.EnumerateModsAsync())
                await Dispatcher.EnqueueAsync(() => Mods.Add(saveInfo));
        }
        catch { }
    }
}
