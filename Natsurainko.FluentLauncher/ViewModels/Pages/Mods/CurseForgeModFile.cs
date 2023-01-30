using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Natsurainko.FluentCore.Model.Mod.CureseForge;
using Natsurainko.FluentCore.Module.Mod;
using Natsurainko.FluentLauncher.Components.FluentCore;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Views.Pages;
using Natsurainko.Toolkits.Network.Downloader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WinUIEx;

namespace Natsurainko.FluentLauncher.ViewModels.Pages.Mods;

public partial class CurseForgeModFile : ObservableObject
{
    public CurseForge.Resource Resource { get; set; }

    public CurseForgeModFile(CurseForge.Resource resource)
    {
        Resource = resource;

        fileInfos = resource.Data.LatestFilesIndexes;
    }

    [ObservableProperty]
    private IEnumerable<CurseForgeResourceFileInfo> fileInfos;

    [RelayCommand]
    public void Download(CurseForgeResourceFileInfo resourceFileInfo)
    {
        var saveFileDialog = new SaveFileDialog();
        saveFileDialog.FileName = resourceFileInfo.FileName;

        var folders = new List<string>();

        if (!string.IsNullOrEmpty(App.Configuration.CurrentGameFolder))
            if (!string.IsNullOrEmpty(App.Configuration.CurrentGameCore))
                if (new GameCoreLocator(App.Configuration.CurrentGameFolder).GetGameCore(App.Configuration.CurrentGameCore) is GameCore core)
                {
                    folders.Add(Path.Combine(core.GetLaunchSetting().WorkingFolder.FullName, "mods"));
                    folders.Add(core.GetLaunchSetting().WorkingFolder.FullName);
                }

        folders.Add(Path.Combine(App.Configuration.CurrentGameFolder, "mods"));
        folders.Add(App.Configuration.CurrentGameFolder);

        foreach (var folder in folders)
            if (Directory.Exists(folder))
            {
                saveFileDialog.InitialDirectory = folder;
                break;
            }

        if (saveFileDialog.ShowDialog().GetValueOrDefault(false))
        {
            ModDownloadArrangement.StartNew(resourceFileInfo.FileName, () => new DownloadRequest()
            {
                Directory = new FileInfo(saveFileDialog.FileName).Directory,
                FileName = resourceFileInfo.FileName,
                Url = CurseForgeApi.GetModFileDownloadUrl(Resource.Data.Id, resourceFileInfo.FileId).GetAwaiter().GetResult()
            });
        }
        else MainContainer.ShowMessagesAsync($"Cancelled Download Mod {resourceFileInfo.FileName}");

        /*
        var savePicker = new FileSavePicker();

        savePicker.SuggestedStartLocation = PickerLocationId.Desktop;
        savePicker.FileTypeChoices.Add("File", new List<string>() { Path.GetExtension(resourceFileInfo.FileName) });
        savePicker.SuggestedFileName = resourceFileInfo.FileName;

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

        var file = await savePicker.PickSaveFileAsync();

        if (file != null)
        {
            ModDownloadArrangement.StartNew(resourceFileInfo.FileName, Task.Run(() =>
            {
                
            }));
        }
        else MainContainer.ShowMessagesAsync($"Cancelled Download Mod {resourceFileInfo.FileName}");
        */
    }
}
