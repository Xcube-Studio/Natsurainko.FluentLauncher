using Nrk.FluentCore.Classes.Datas.Download;
using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Datas.Parse;
using Nrk.FluentCore.DefaultComponents.Download;
using Nrk.FluentCore.DefaultComponents.Parse;
using Nrk.FluentCore.Interfaces.ServiceInterfaces;
using Nrk.FluentCore.Utils;
using System.Collections.Generic;
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

    public virtual DefaultResourcesDownloader CreateResourcesDownloader(GameInfo gameInfo,
        IEnumerable<LibraryElement> libraryElements = default,
        IEnumerable<AssetElement> assetElements = default,
        DownloadMirrorSource downloadMirrorSource = default)
    {
        List<LibraryElement> libraries = libraryElements.ToList();

        if (libraryElements == null)
        {
            var libraryParser = new DefaultLibraryParser(gameInfo);
            libraryParser.EnumerateLibraries(out var enabledLibraries, out var enabledNativesLibraries);

            libraries = enabledLibraries.Union(enabledNativesLibraries).ToList();
        }

        if (assetElements == null)
        {
            var assetParser = new DefaultAssetParser(gameInfo);
            var assetElement = assetParser.GetAssetIndexJson();
            if (downloadMirrorSource != null) assetElement.Url.ReplaceFromDictionary(downloadMirrorSource.AssetsReplaceUrl);

            if (!assetElement.VerifyFile())
            {
                var assetIndexDownloadTask = HttpUtils.DownloadElementAsync(assetElement);
                assetIndexDownloadTask.Wait();

                if (assetIndexDownloadTask.Result.IsFaulted)
                    throw new System.Exception("依赖材质索引文件获取失败");
            }

            assetElements = assetParser.EnumerateAssets();
        }

        var jar = gameInfo.GetJarElement();
        if (jar != null && !jar.VerifyFile())
            libraries.Add(jar);

        var defaultResourcesDownloader = new DefaultResourcesDownloader(gameInfo);

        defaultResourcesDownloader.SetLibraryElements(libraries);
        defaultResourcesDownloader.SetAssetsElements(assetElements);

        if (downloadMirrorSource != null) defaultResourcesDownloader.SetDownloadMirror(downloadMirrorSource);

        return defaultResourcesDownloader;
    }
}
