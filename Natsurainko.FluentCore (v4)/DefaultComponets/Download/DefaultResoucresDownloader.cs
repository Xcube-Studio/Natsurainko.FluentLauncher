using Nrk.FluentCore.Classes.Datas.Download;
using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Classes.Datas.Parse;
using Nrk.FluentCore.Components.Download;
using Nrk.FluentCore.Interfaces;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace Nrk.FluentCore.DefaultComponets.Download;

public class DefaultResoucresDownloader : BaseResourcesDownloader
{
    protected DownloadMirrorSource _downloadMirrorSource;
    protected bool _enableUseDownloadSource;

    protected readonly DownloadSetting _downloadSetting = HttpUtils.DownloadSetting;
    protected readonly List<DownloadResult> _errorDownload = new();

    public event EventHandler<int> DownloadElementsPosted;

    public IReadOnlyList<DownloadResult> ErrorDownload => _errorDownload;

    public DefaultResoucresDownloader(GameInfo gameInfo) : base(gameInfo) { }

    public override void Download(CancellationTokenSource tokenSource = default)
    {
        var filteredLibraries = _libraryElements.AsParallel().Where(x => !x.VerifyFile()).ToList();
        var filteredAssets = _assetElements.AsParallel().Where(x => !x.VerifyFile()).ToList();

        var transformBlock = new TransformBlock<IDownloadElement, IDownloadElement>(e =>
        {
            if (_enableUseDownloadSource)
            {
                if (e is LibraryElement library)
                    e.Url = e.Url.ReplaceFromDictionary(_downloadMirrorSource.LibrariesReplaceUrl);

                if (e is AssetElement asset)
                    e.Url = e.Url.ReplaceFromDictionary(_downloadMirrorSource.AssetsReplaceUrl);
            }

            return e;
        }, new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = _downloadSetting.MultiThreadsCount,
            MaxDegreeOfParallelism = _downloadSetting.MultiThreadsCount,
            CancellationToken = tokenSource.Token
        });

        var actionBlock = new ActionBlock<IDownloadElement>(async e =>
        {
            if (string.IsNullOrEmpty(e.Url))
            {
                _errorDownload.Add(new DownloadResult
                {
                    DownloadElement = e,
                    Exception = new Exception("该依赖库无法被下载"),
                    IsFaulted = true
                });

                return;
            }

            var downloadResult = await HttpUtils.DownloadElementAsync(e, tokenSource: tokenSource);
            if (downloadResult.IsFaulted) _errorDownload.Add(downloadResult);

            OnSingleFileDownloaded();
        },
        new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = _downloadSetting.MultiThreadsCount,
            MaxDegreeOfParallelism = _downloadSetting.MultiThreadsCount,
            CancellationToken = tokenSource.Token
        });

        var transformManyBlock = new TransformManyBlock<IEnumerable<IDownloadElement>, IDownloadElement>(chunk => chunk,
            new ExecutionDataflowBlockOptions());

        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

        transformManyBlock.LinkTo(transformBlock, linkOptions);
        transformBlock.LinkTo(actionBlock, linkOptions);

        transformManyBlock.Post(filteredLibraries);
        transformManyBlock.Post(filteredAssets);

        DownloadElementsPosted?.Invoke(this, filteredLibraries.Count + filteredAssets.Count);

        actionBlock.Complete();
        actionBlock.Completion.Wait();
    }

    public override void SetAssetsElements(IEnumerable<AssetElement> assetElements)
    {
        _assetElements = assetElements;
    }

    public override void SetLibraryElements(IEnumerable<LibraryElement> libraryElements)
    {
        _libraryElements = libraryElements;
    }

    public void SetDownloadMirror(DownloadMirrorSource mirrorSource)
    {
        _downloadMirrorSource = mirrorSource;
        _enableUseDownloadSource = true;
    }
}
