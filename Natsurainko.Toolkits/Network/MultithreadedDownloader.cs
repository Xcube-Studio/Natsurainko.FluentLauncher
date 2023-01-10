using Natsurainko.Toolkits.Network.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Natsurainko.Toolkits.Network
{
    public class MultithreadedDownloader<T>
    {
        public event EventHandler Completed;

        public event EventHandler<HttpDownloadResponse> SingleDownloaded;

        public event EventHandler<(float, string)> ProgressChanged;

        public int MaxThreadNumber { get; set; } = 128;

        public int BufferCapacity { get; set; } = 256;

        public List<T> Sources { get; private set; }

        public Func<T, HttpDownloadRequest> HandleFunc { get; private set; }

        private readonly Dictionary<HttpDownloadRequest, HttpDownloadResponse> FailedDownloadRequests = new();

        public MultithreadedDownloader(Func<T, HttpDownloadRequest> func, List<T> sources)
        {
            HandleFunc = func;
            Sources = sources;
        }

        public async Task<MultithreadedDownloadResponse> DownloadAsync()
        {
            var manyBlock = new TransformManyBlock<List<T>, HttpDownloadRequest>(x => x.Select(x => HandleFunc(x)));

            int post = 0;
            int output = 0;

            var actionBlock = new ActionBlock<HttpDownloadRequest>(async request =>
            {
                post++;

                if (!request.Directory.Exists)
                    request.Directory.Create();

                HttpDownloadResponse httpDownloadResponse = null;
                try
                {
                    httpDownloadResponse = await HttpWrapper.HttpDownloadAsync(request);

                    if (httpDownloadResponse.HttpStatusCode != HttpStatusCode.OK)
                        FailedDownloadRequests.Add(request, httpDownloadResponse);

                    this.SingleDownloaded?.Invoke(this, httpDownloadResponse);
                }
                catch
                {
                    if (httpDownloadResponse.HttpStatusCode != HttpStatusCode.OK)
                        FailedDownloadRequests.Add(request, httpDownloadResponse);
                }

                output++;

                ProgressChanged?.Invoke(this, (output / (float)post, $"{output}/{post}"));
            }, new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = BufferCapacity,
                MaxDegreeOfParallelism = MaxThreadNumber
            });
            var disposable = manyBlock.LinkTo(actionBlock, new DataflowLinkOptions { PropagateCompletion = true });

            manyBlock.Post(this.Sources);
            manyBlock.Complete();

            await actionBlock.Completion;
            Completed?.Invoke(this, null);

            disposable.Dispose();
            GC.Collect();

            return new MultithreadedDownloadResponse
            {
                IsAllSuccess = !FailedDownloadRequests.Any(),
                FailedDownloadRequests = FailedDownloadRequests
            };
        }
    }
}
