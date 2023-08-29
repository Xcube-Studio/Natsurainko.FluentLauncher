using Natsurainko.FluentLauncher.Classes.Data.UI;
using Natsurainko.FluentLauncher.Services.Settings;
using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Datas.Parse;
using Nrk.FluentCore.DefaultComponents.Download;
using Nrk.FluentCore.Services.Download;
using Nrk.FluentCore.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Natsurainko.FluentLauncher.Services.Download;

internal class DownloadService : DefaultDownloadService
{
    private new readonly SettingsService _settingsService;
    private readonly ObservableCollection<DownloadProcess> _downloadProcesses = new();

    public ReadOnlyObservableCollection<DownloadProcess> DownloadProcesses { get; init; }

    public DownloadService(SettingsService settingsService) : base(settingsService)
    {
        _settingsService = settingsService;

        DownloadProcesses = new(_downloadProcesses);
    }

    public DefaultResourcesDownloader CreateResourcesDownloader(GameInfo gameInfo, IEnumerable<LibraryElement> libraryElements = null)
    {
        UpdateDownloadSettings();

        if (_settingsService.CurrentDownloadSource != "Mojang")
            return base.CreateResourcesDownloader(gameInfo, libraryElements, downloadMirrorSource:
                _settingsService.CurrentDownloadSource.Equals("Mcbbs") ? DownloadMirrors.Mcbbs : DownloadMirrors.Bmclapi);

        return base.CreateResourcesDownloader(gameInfo, libraryElements);
    }

    private void UpdateDownloadSettings()
    {
        HttpUtils.DownloadSetting.EnableLargeFileMultiPartDownload = _settingsService.EnableFragmentDownload;
        HttpUtils.DownloadSetting.MultiThreadsCount = _settingsService.MaxDownloadThreads;
    }

    public void CreateDownloadProcessFromResourceFile(object file, string filePath)
    {
        var process = new ResourceDownloadProcess(file, filePath);
        _downloadProcesses.Insert(0, process);

        _ = process.Start();
    }
}
