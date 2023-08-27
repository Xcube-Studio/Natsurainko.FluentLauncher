using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentLauncher.Classes.Data.Download;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Views.Downloads;
using Natsurainko.FluentLauncher.Views;
using Nrk.FluentCore.Classes.Datas;
using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.DefaultComponents.Manage;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;

namespace Natsurainko.FluentLauncher.ViewModels.Cores.Manage;

internal partial class CoreModsViewModel : ObservableObject
{
    public readonly DefaultModsManager modsManager;

    public CoreModsViewModel(GameInfo gameInfo)
    {
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

    public bool SupportMod { get; init; }

    public string ModsFolder { get; init; }

    private IReadOnlyList<ModInfo> Mods { get; set; }

    [ObservableProperty]
    private IEnumerable<ModInfo> displayMods;

    [ObservableProperty]
    private string searchBoxInput;

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
        => ShellPage.ContentFrame.Navigate(typeof(ResourcesSearchPage), new ResourceSearchData
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
