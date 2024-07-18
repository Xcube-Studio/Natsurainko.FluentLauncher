using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Models.Download;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.Management;
using Nrk.FluentCore.Management.Mods;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;

namespace Natsurainko.FluentLauncher.ViewModels.Cores.Manage;

internal partial class CoreModsViewModel : ObservableObject, INavigationAware
{
    public DefaultModsManager modsManager;
    private readonly INavigationService _navigationService;

    public CoreModsViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public bool SupportMod { get; private set; }

    public string ModsFolder { get; private set; }

    private IReadOnlyList<ModInfo> Mods { get; set; }

    [ObservableProperty]
    private IEnumerable<ModInfo> displayMods;

    [ObservableProperty]
    private string searchBoxInput;

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        if (parameter is not GameInfo gameInfo)
            throw new ArgumentException("Invalid parameter type");

        SupportMod = gameInfo.IsSupportMod();

        if (SupportMod)
        {
            ModsFolder = Path.Combine(gameInfo.GetGameDirectory(), "mods");
            if (!Directory.Exists(ModsFolder)) Directory.CreateDirectory(ModsFolder);
            modsManager = new DefaultModsManager(ModsFolder);

            Task.Run(() =>
            {
                Mods = modsManager.EnumerateMods().ToList();
                UpdateDisplayMods();
            });
        }
    }

    [RelayCommand]
    public void OpenFolder() => _ = Launcher.LaunchFolderPathAsync(ModsFolder);

    private void UpdateDisplayMods()
    {
        IEnumerable<ModInfo> mods = Mods;

        if (!string.IsNullOrEmpty(SearchBoxInput))
            mods = mods.Where(x => x.DisplayName.ToLower().Contains(SearchBoxInput.ToLower()));

        mods = mods.ToList();

        App.DispatcherQueue.TryEnqueue(() => DisplayMods = mods);
    }

    [RelayCommand]
    public void SearchAllMinecraft()
        => _navigationService.Parent.NavigateTo("ResourcesSearchPage", new ResourceSearchData
        {
            SearchInput = string.Empty,
            ResourceType = 0
        });


    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(SearchBoxInput))
            Task.Run(UpdateDisplayMods);
    }
}
