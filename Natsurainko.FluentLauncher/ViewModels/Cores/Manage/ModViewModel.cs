using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Natsurainko.FluentLauncher.Utils;
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
    public DefaultModsManager modsManager;

    public string ModsFolder { get; private set; }

    public GameInfo GameInfo { get; private set; }

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

        modsManager = new DefaultModsManager(ModsFolder);

        Task.Run(() =>
        {
            var modInfos = modsManager.EnumerateMods().ToList();
            App.DispatcherQueue.TryEnqueue(() => Mods = modInfos);
        });
    }

    [RelayCommand]
    public async Task OpenModsFolder() => await Launcher.LaunchFolderPathAsync(ModsFolder);

    [RelayCommand]
    public void Toggled(object args)
    {
        var toggleSwitch = args.As<ToggleSwitch, object>().sender;

        var modInfo = (ModInfo)toggleSwitch.DataContext;

        if (modInfo != null)
            modsManager.Switch(modInfo, toggleSwitch.IsOn);
    }
}
