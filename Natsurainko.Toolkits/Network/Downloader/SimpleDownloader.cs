using Natsurainko.Toolkits.Network.Downloader.Model;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Natsurainko.Toolkits.Network.Downloader
{
    public class SimpleDownloader : DownloaderBase<SimpleDownloaderResponse, SimpleDownloaderProgressChangedEventArgs>
    {
        public SimpleDownloader(string url, string downloadFolder, string targetFileName = null)
        {
            this.Url = url;

            this.DownloadFolder = downloadFolder;
            this.TargetFileName = targetFileName;
        }

        public SimpleDownloader(string url, string downloadFolder, string targetFileName = null, long? rangeFrom = null, long? rangeTo = null)
        {
            this.Url = url;

            this.DownloadFolder = downloadFolder;
            this.TargetFileName = targetFileName;

            this.RangeFrom = rangeFrom;
            this.RangeTo = rangeTo;
        }

        public long TotleLength { get; private set; }

        public long CompletedLength { get; private set; }

        public string Url { get; private set; }

        public string TargetFileName { get; private set; }

        public string DownloadFolder { get; private set; }

        private long? RangeFrom;

        private long? RangeTo;

        public HttpResponseMessage HttpResponseMessage { get; private set; }

        public override void BeginDownload()
        {
            base.BeginDownload();

            DownloadProcess = Task.Run(async () =>
            {
                if (RangeFrom.HasValue && RangeTo.HasValue)
                    HttpResponseMessage = await HttpWrapper.HttpGetAsync
                        (Url, new Dictionary<string, string>() { { "Range", $"bytes={RangeFrom}-{RangeTo}" } }, HttpCompletionOption.ResponseHeadersRead);
                else HttpResponseMessage = await HttpWrapper.HttpGetAsync(Url, new Dictionary<string, string>(), HttpCompletionOption.ResponseHeadersRead);
                TotleLength = (long)HttpResponseMessage.Content.Headers.ContentLength;

                if (HttpResponseMessage.Content.Headers.ContentDisposition != null && !string.IsNullOrEmpty(HttpResponseMessage.Content.Headers.ContentDisposition.FileName) && string.IsNullOrEmpty(TargetFileName))
                    TargetFileName = HttpResponseMessage.Content.Headers.ContentDisposition.FileName.Trim('\"');

                if (string.IsNullOrEmpty(TargetFileName))
                    TargetFileName = Path.GetFileName(HttpResponseMessage.RequestMessage.RequestUri.AbsoluteUri);

                HttpResponseMessage.EnsureSuccessStatusCode();

                using var stream = await HttpResponseMessage.Content.ReadAsStreamAsync();
                using var fileStream = File.Create(Path.Combine(DownloadFolder, TargetFileName));

                byte[] buffer = new byte[ReadBufferSize];
                int readSize = 0;

                async Task<bool> Read() { readSize = await stream.ReadAsync(buffer, 0, buffer.Length); return readSize > 0; };

                while (await Read())
                {
                    await fileStream.WriteAsync(buffer, 0, readSize);
                    CompletedLength += readSize;

                    OnProgressChanged(new SimpleDownloaderProgressChangedEventArgs
                    {
                        CompletedLength = CompletedLength,
                        TotleLength = TotleLength
                    });
                }

                await fileStream.FlushAsync();
            }).ContinueWith(task =>
            {
                DownloadTimeStopwatch.Stop();

                if (task.IsFaulted)
                    OnDownloadFailed(task.Exception);

                var simpleDownloaderResponse = new SimpleDownloaderResponse
                {
                    Exception = task.Exception,
                    Success = !task.IsFaulted,
                    CompletionType = task.IsFaulted ? DownloaderCompletionType.Uncompleted : DownloaderCompletionType.AllCompleted,
                    DownloadTime = DownloadTimeStopwatch.Elapsed,
                    Result = new FileInfo(Path.Combine(DownloadFolder, TargetFileName))
                };

                OnDownloadCompleted(simpleDownloaderResponse);
                return simpleDownloaderResponse;
            });
        }

        public override async Task<SimpleDownloaderResponse> CompleteAsync() => await DownloadProcess;

        public override void Dispose()
        {
            HttpResponseMessage.Dispose();
            DownloadProcess.Dispose();

            HttpResponseMessage = null;
            DownloadProcess = null;
        }
    }
}
