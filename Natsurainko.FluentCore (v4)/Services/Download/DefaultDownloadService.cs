using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Datas.Parse;
using Nrk.FluentCore.DefaultComponets.Download;
using Nrk.FluentCore.DefaultComponets.Parse;
using Nrk.FluentCore.Interfaces.ServiceInterfaces;
using Nrk.FluentCore.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nrk.FluentCore.Services.Download;

/// <summary>
/// 下载服务的默认实现（IoC适应）
/// </summary>
public class DefaultDownloadService
{
    protected readonly IFluentCoreSettingsService _settingsService;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="settingsService">实际使用时请使用具体的继承类型替代之</param>
    public DefaultDownloadService(IFluentCoreSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public DefaultResoucresDownloader CreateResoucresDownloader(GameInfo gameInfo, 
        IEnumerable<LibraryElement> libraryElements = default,
        IEnumerable<AssetElement> assetElements = default)
    {
        if (libraryElements == null)
        {
            var libraryParser = new DefaultLibraryParser(gameInfo);
            libraryParser.EnumerateLibraries(out var enabledLibraries, out var enabledNativesLibraries);

            libraryElements = enabledLibraries.Union(enabledNativesLibraries);
        }

        if (assetElements == null)
        {
            var assetParser = new DefaultAssetParser(gameInfo);
            var assetElement = assetParser.GetAssetIndexJson();

            if (!File.Exists(assetElement.AbsolutePath) && )
            {
                var assetIndexDownloadTask = HttpUtils.DownloadElementAsync(assetElement);
                assetIndexDownloadTask.Wait();

                if (assetIndexDownloadTask.Result.IsFaulted)
                    throw new System.Exception("依赖材质索引文件获取失败");
            }


        }

        if (gameInfo.IsVanilla)
    }
}
