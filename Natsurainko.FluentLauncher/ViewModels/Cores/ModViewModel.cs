using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

public partial class ModViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;

    public ModManager ModsManager { get; private set; }

    public string ModsFolder { get; private set; }

    public MinecraftInstance MinecraftInstance { get; private set; }

    public bool NotSupportMod => !MinecraftInstance.IsSupportMod();

    public ObservableCollection<MinecraftMod> Mods { get; private set; } = [];

    public ModViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

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
    public async Task OpenModsFolder() => await Launcher.LaunchFolderPathAsync(ModsFolder);

    [RelayCommand]
    public void DeleteMod(MinecraftMod modInfo)
    {
        File.Delete(modInfo.AbsolutePath);
        LoadModList();
    }

    private async void LoadModList()
    {
        try
        {
            Mods.Clear();

            await foreach (var saveInfo in ModsManager.EnumerateModsAsync())
                App.DispatcherQueue.TryEnqueue(() => Mods.Add(saveInfo));
        }
        catch
        {

        }
    }
}
