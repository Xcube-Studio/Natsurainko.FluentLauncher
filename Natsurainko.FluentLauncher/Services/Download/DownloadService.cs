using Natsurainko.FluentLauncher.Services.Settings;
using Nrk.FluentCore.Classes.Datas.Download;
using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Datas.Parse;
using Nrk.FluentCore.DefaultComponets.Download;
using Nrk.FluentCore.Interfaces.ServiceInterfaces;
using Nrk.FluentCore.Services.Download;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.Download;

public class DownloadService : DefaultDownloadService
{
    private new readonly SettingsService _settingsService;

    public DownloadService(SettingsService settingsService) : base(settingsService)
    {
        _settingsService= settingsService;
    }

    public DefaultResoucresDownloader CreateResoucresDownloader(GameInfo gameInfo, IEnumerable<LibraryElement> libraryElements = null)
    {
        if (_settingsService.CurrentDownloadSource != "Mojang")
            return base.CreateResoucresDownloader(gameInfo, libraryElements, downloadMirrorSource: 
                _settingsService.CurrentDownloadSource.Equals("Mcbbs") ? DownloadMirrors.Mcbbs : DownloadMirrors.Bmclapi);

        return base.CreateResoucresDownloader(gameInfo, libraryElements);
    }
}
