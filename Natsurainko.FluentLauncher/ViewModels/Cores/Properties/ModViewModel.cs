using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Components.FluentCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Cores.Properties;

partial class ModViewModel : ObservableObject
{
    public ModViewModel(GameCore core)
    {
        Core = core;
        Load();
    }

    [ObservableProperty]
    public string modsFolder;

    [ObservableProperty]
    public IEnumerable<ModInfo> mods;

    [ObservableProperty]
    private Visibility tipVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility modsVisibility = Visibility.Visible;

    [ObservableProperty]
    private string tipTitle;

    [ObservableProperty]
    private string tipSubTitle;

    [ObservableProperty]
    private bool deleteIsOpen;

    [ObservableProperty]
    private ModInfo toDelete;

    private GameCore Core { get; set; }

    private void Load()
    {
        var launchSetting = Core.GetLaunchSetting();

        var modsFolder = new DirectoryInfo(Path.Combine(
            launchSetting.EnableIndependencyCore ? launchSetting.WorkingFolder.FullName : Core.Root.FullName,
            "mods"));

        if (Core.IsVanilla)
        {
            TipVisibility = Visibility.Visible;
            ModsVisibility = Visibility.Collapsed;

            TipTitle = "This Core does Not Support Mods";
            TipSubTitle = "Install New Core";

            return;
        }

        if (modsFolder.Exists)
        {
            ModsFolder = modsFolder.FullName;

            Task.Run(() =>
            {
                var mods = ModInfoReader.GetModInfos(modsFolder).ToList();
                App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    Mods = mods;

                    if (!Mods.Any())
                    {
                        TipVisibility = Visibility.Visible;
                        ModsVisibility = Visibility.Collapsed;

                        TipTitle = "No Mod";
                        TipSubTitle = "Go to Mods to Download";
                    }
                });
            });
        }
        else
        {
            TipVisibility = Visibility.Visible;
            ModsVisibility = Visibility.Collapsed;

            TipTitle = "No Mod";
            TipSubTitle = "Go to Mods to Download";
        }
    }

    [RelayCommand]
    private void Delete()
    {
        if (ToDelete.File.Exists)
            ToDelete.File.Delete();

        Load();

        DeleteIsOpen = false;
        ToDelete = null;
    }

    [RelayCommand]
    public void OpenDelete(ModInfo data)
    {
        if (data.Equals(ToDelete))
            OnPropertyChanged(nameof(ToDelete));

        ToDelete = data;
        DeleteIsOpen = true;
    }
}
