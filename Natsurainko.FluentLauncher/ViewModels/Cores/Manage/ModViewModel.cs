using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.Management;
using Nrk.FluentCore.Management.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Cores.Manage;

public partial class ModViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;

    public DefaultModsManager ModsManager { get; private set; }

    public string ModsFolder { get; private set; }

    public GameInfo GameInfo { get; private set; }

    public bool NotSupportMod => !GameInfo.IsSupportMod();

    [ObservableProperty]
    public IReadOnlyList<ModInfo> mods;

    public ModViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        GameInfo = parameter as GameInfo;
        ModsFolder = GameInfo.GetModsDirectory();

        if (!Directory.Exists(ModsFolder))
            Directory.CreateDirectory(ModsFolder);

        ModsManager = new DefaultModsManager(ModsFolder);

        LoadModList();
    }

    [RelayCommand]
    public async Task OpenModsFolder() => await Launcher.LaunchFolderPathAsync(ModsFolder);

    [RelayCommand]
    public void DeleteMod(ModInfo modInfo)
    {
        File.Delete(modInfo.AbsolutePath);
        LoadModList();
    }

    private void LoadModList()
    {
        Task.Run(() =>
        {
            var modInfos = ModsManager.EnumerateMods().ToList();
            App.DispatcherQueue.TryEnqueue(() => Mods = modInfos);
        });
    }
}
