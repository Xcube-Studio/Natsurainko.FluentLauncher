using Natsurainko.FluentLauncher.Services.Settings;
using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Datas.Parse;
using Nrk.FluentCore.DefaultComponets.Download;
using Nrk.FluentCore.Services.Download;
using Nrk.FluentCore.Utils;
using System.Collections.Generic;

namespace Natsurainko.FluentLauncher.Services.Download;

public class DownloadService : DefaultDownloadService
{
    private new readonly SettingsService _settingsService;

    public DownloadService(SettingsService settingsService) : base(settingsService)
    {
        _settingsService= settingsService;
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
}
