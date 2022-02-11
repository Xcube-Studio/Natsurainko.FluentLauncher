using FluentCore.Interface;
using FluentCore.Model;
using FluentCore.Model.Launch;
using FluentCore.Service.Local;
using FluentCore.Service.Network;
using FluentCore.Service.Network.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace FluentCore.Service.Component.DependencesResolver
{
    /// <summary>
    /// 游戏依赖补全器
    /// </summary>
    public class DependencesCompleter
    {
        /// <summary>
        /// 游戏核心
        /// </summary>
        public GameCore GameCore { get; set; }

        /// <summary>
        /// 最大线程数
        /// </summary>
        public static int MaxThread { get; set; } = 64;

        public int NeedDownloadDependencesCount { get; set; }

        public DependencesCompleter(GameCore core) => this.GameCore = core;

        /// <summary>
        /// 下载错误返回集合
        /// </summary>
        public List<HttpDownloadResponse> ErrorDownloadResponses = new List<HttpDownloadResponse>();

        /// <summary>
        /// 单个文件下载完事件
        /// </summary>
        public event EventHandler<HttpDownloadResponse> SingleDownloadedEvent;

        /// <summary>
        /// 补全游戏文件(异步)
        /// </summary>
        /// <returns></returns>
        public async Task CompleteAsync()
        {
            var mainJarRequest = GetMainJarDownloadRequest();
            if (mainJarRequest != null)
            {
                var res = await HttpHelper.HttpDownloadAsync(mainJarRequest, "client.jar");
                File.Move(res.FileInfo.FullName, this.GameCore.MainJar);
            }

            var manyBlock = new TransformManyBlock<IEnumerable<HttpDownloadRequest>, HttpDownloadRequest>(x => x);
            var blockOptions = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = MaxThread,
                MaxDegreeOfParallelism = MaxThread
            };

            var actionBlock = new ActionBlock<HttpDownloadRequest>(async x =>
            {
                if (!x.Directory.Exists)
                    x.Directory.Create();

                var res = await HttpHelper.HttpDownloadAsync(x, x.FileName);
                if (res.HttpStatusCode != HttpStatusCode.OK)
                    this.ErrorDownloadResponses.Add(res);

                SingleDownloadedEvent?.Invoke(this, res);
            }, blockOptions);

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            var dis = manyBlock.LinkTo(actionBlock, linkOptions);

            var req = await GetRequestsAsync();
            this.NeedDownloadDependencesCount = req.Count();

            _ = manyBlock.Post(req);
            manyBlock.Complete();

            await actionBlock.Completion;
            dis.Dispose();

            GC.Collect();
        }

        /// <summary>
        /// 获取所有下载(除主Jar外)请求(异步)
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<HttpDownloadRequest>> GetRequestsAsync()
        {
            var dependences = await new AssetsResolver(this.GameCore).GetLostDependencesAsync();
            dependences = dependences.Union(new LibrariesResolver(this.GameCore).GetLostDependences());

            var requests = new List<HttpDownloadRequest>();

            foreach (IDependence dependence in dependences)
                requests.Add(dependence.GetDownloadRequest(this.GameCore.Root));

            return requests;
        }

        /// <summary>
        /// 获取游戏主Jar下载请求(异步)
        /// </summary>
        /// <returns></returns>
        public HttpDownloadRequest GetMainJarDownloadRequest()
        {
            var file = new FileInfo(this.GameCore.MainJar);
            var model = this.GameCore.Downloads["client"];

            if (!file.Exists)
            {
                return new HttpDownloadRequest
                {
                    Directory = file.Directory,
                    Url = SystemConfiguration.Api != new Mojang() ? model.Url.Replace("https://launcher.mojang.com", SystemConfiguration.Api.Url) : model.Url,
                    Sha1 = model.Sha1,
                    Size = model.Size
                };
            }

            return null;
        }
    }
}
