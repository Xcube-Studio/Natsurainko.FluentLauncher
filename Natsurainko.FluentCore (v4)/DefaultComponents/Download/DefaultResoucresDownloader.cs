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

namespace Nrk.FluentCore.DefaultComponents.Download;

public class DefaultResourcesDownloader : BaseResourcesDownloader
{
    protected DownloadMirrorSource _downloadMirrorSource;
    protected bool _enableUseDownloadSource;

    protected readonly DownloadSetting _downloadSetting = HttpUtils.DownloadSetting;
    protected readonly List<DownloadResult> _errorDownload = new();

    public event EventHandler<int> DownloadElementsPosted;

    public IReadOnlyList<DownloadResult> ErrorDownload => _errorDownload;

    public DefaultResourcesDownloader(GameInfo gameInfo) : base(gameInfo) { }

    public override void Download(CancellationTokenSource tokenSource = default)
    {
        tokenSource ??= new CancellationTokenSource();

        var filteredLibraries = _libraryElements.AsParallel().Where(x => !x.VerifyFile()).ToList();
        var filteredAssets = _assetElements.AsParallel().Where(x => !x.VerifyFile()).ToList(); //TODO: assets 去重！

        var transformBlock = new TransformBlock<IDownloadElement, IDownloadElement>(e =>
        {
            if (string.IsNullOrEmpty(e.Url))
                return e;

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

            await HttpUtils.DownloadElementAsync(e, tokenSource: tokenSource).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    if (!e.VerifyFile())
                        _errorDownload.Add(new DownloadResult
                        {
                            DownloadElement = e,
                            Exception = task.Exception,
                            IsFaulted = true
                        });

                    return;
                }

                var downloadResult = task.Result;

                if (downloadResult.IsFaulted)
                {
                    if (!downloadResult.DownloadElement.VerifyFile())
                        _errorDownload.Add(downloadResult);
                }
            });

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
        transformManyBlock.Complete();

        DownloadElementsPosted?.Invoke(this, filteredLibraries.Count + filteredAssets.Count);

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
