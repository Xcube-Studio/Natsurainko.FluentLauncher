using Natsurainko.FluentCore.Class.Model.Download;
using Natsurainko.FluentCore.Class.Model.Launch;
using Natsurainko.FluentCore.Class.Model.Parser;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Module.Parser;
using Natsurainko.Toolkits.IO;
using Natsurainko.Toolkits.Network;
using Natsurainko.Toolkits.Network.Model;
using Natsurainko.Toolkits.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Natsurainko.FluentCore.Module.Downloader;

public class ResourceDownloader : IResourceDownloader
{
    public GameCore GameCore { get; set; }

    public Action<string, float> DownloadProgressChangedAction { get; set; }

    public List<IResource> FailedResources { get; set; } = new List<IResource>();

    public static int MaxDownloadThreads { get; set; } = 128;

    public ResourceDownloader() { }

    public ResourceDownloader(GameCore core)
    {
        this.GameCore = core;
    }

    public event EventHandler<HttpDownloadResponse> ItemDownloaded;

    public async Task<ResourceDownloadResponse> DownloadAsync(Action<string, float> func)
    {
        var progress = new Progress<(string, float)>();

        void Progress_ProgressChanged(object _, (string, float) e) => func(e.Item1, e.Item2);

        progress.ProgressChanged += Progress_ProgressChanged;

        var manyBlock = new TransformManyBlock<List<IResource>, IResource>(x => x.Where(x =>
        {
            if (string.IsNullOrEmpty(x.CheckSum) && x.Size == 0)
                return false;
            if (x.ToFileInfo().Verify(x.CheckSum) && x.ToFileInfo().Verify(x.Size))
                return false;

            return true;
        }));

        int post = 0;
        int output = 0;

        var actionBlock = new ActionBlock<IResource>(async resource =>
        {
            post++;
            var request = resource.ToDownloadRequest();

            if (!request.Directory.Exists)
                request.Directory.Create();

            try
            {
                var httpDownloadResponse = await HttpWrapper.HttpDownloadAsync(request);

                if (httpDownloadResponse.HttpStatusCode != HttpStatusCode.OK)
                    this.FailedResources.Add(resource);

                this.ItemDownloaded?.Invoke(this, httpDownloadResponse);
            }
            catch
            {
                this.FailedResources.Add(resource);
            }

            output++;

            ((IProgress<(string, float)>)progress).Report(($"{output}/{post}", output / (float)post));
        }, new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = MaxDownloadThreads,
            MaxDegreeOfParallelism = MaxDownloadThreads
        });
        var disposable = manyBlock.LinkTo(actionBlock, new DataflowLinkOptions { PropagateCompletion = true });

        manyBlock.Post(this.GameCore.LibraryResources.Where(x => x.IsEnable).Select(x => (IResource)x).ToList());
        manyBlock.Post(this.GetFileResources().ToList());
        manyBlock.Post(await this.GetAssetResourcesAsync());

        manyBlock.Complete();

        await actionBlock.Completion;
        disposable.Dispose();

        GC.Collect();

        progress.ProgressChanged -= Progress_ProgressChanged;

        return new ResourceDownloadResponse
        {
            FailedResources = this.FailedResources,
            SuccessCount = post - this.FailedResources.Count,
            Total = post
        };
    }

    public Task<ResourceDownloadResponse> DownloadAsync()
    {
        if (this.DownloadProgressChangedAction != null)
            return this.DownloadAsync(this.DownloadProgressChangedAction);

        return this.DownloadAsync((_, _) => { });
    }

    public IEnumerable<IResource> GetFileResources()
    {
        if (this.GameCore.ClientFile != null)
            yield return this.GameCore.ClientFile;
    }

    public async Task<List<IResource>> GetAssetResourcesAsync()
    {
        if (!(this.GameCore.AssetIndexFile.FileInfo.Verify(this.GameCore.AssetIndexFile.Size)
            || this.GameCore.AssetIndexFile.FileInfo.Verify(this.GameCore.AssetIndexFile.CheckSum)))
        {
            var request = this.GameCore.AssetIndexFile.ToDownloadRequest();

            if (!request.Directory.Exists)
                request.Directory.Create();

            var res = await HttpWrapper.HttpDownloadAsync(request);
        }

        var entity = new AssetManifestJsonEntity();
        entity = entity.FromJson(File.ReadAllText(this.GameCore.AssetIndexFile.ToFileInfo().FullName));

        return new AssetParser(entity, this.GameCore.Root).GetAssets().Select(x => (IResource)x).ToList();
    }
}
