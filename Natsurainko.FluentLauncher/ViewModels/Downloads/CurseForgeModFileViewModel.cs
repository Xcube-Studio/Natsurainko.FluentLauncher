using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Natsurainko.FluentCore.Model.Mod.CureseForge;
using Natsurainko.FluentCore.Module.Mod;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Components.FluentCore;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Services.Data;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.Toolkits.Network.Downloader;
using System.Collections.Generic;
using System.IO;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads;

internal partial class CurseForgeModFileViewModel : ObservableObject
{
    private readonly SettingsService _settings;

    public CurseForgeResourceData Resource { get; }

    public CurseForgeModFileViewModel(CurseForgeResourceData resource, SettingsService settings)
    {
        _settings = settings;
        Resource = resource;

        fileInfos = resource.InnerData.LatestFilesIndexes;
    }

    [ObservableProperty]
    private IEnumerable<CurseForgeResourceFileInfo> fileInfos;

    [RelayCommand]
    public void Download(CurseForgeResourceFileInfo resourceFileInfo)
    {
        var saveFileDialog = new SaveFileDialog();
        saveFileDialog.FileName = resourceFileInfo.FileName;

        var folders = new List<string>();

        if (!string.IsNullOrEmpty(_settings.CurrentGameFolder))
            if (!string.IsNullOrEmpty(_settings.CurrentGameCore))
                if (new GameCoreLocator(_settings.CurrentGameFolder).GetGameCore(_settings.CurrentGameCore) is GameCore core)
                {
                    folders.Add(Path.Combine(core.GetLaunchSetting().WorkingFolder.FullName, "mods"));
                    folders.Add(core.GetLaunchSetting().WorkingFolder.FullName);
                }

        folders.Add(Path.Combine(_settings.CurrentGameFolder, "mods"));
        folders.Add(_settings.CurrentGameFolder);

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
                Url = CurseForgeApi.GetModFileDownloadUrl(Resource.InnerData.Id, resourceFileInfo.FileId).GetAwaiter().GetResult()
            });
        }
        else MessageService.Show($"Cancelled Download Mod {resourceFileInfo.FileName}");
    }
}
