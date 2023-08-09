using Nrk.FluentCore.Classes.Datas.Download;
using Nrk.FluentCore.Interfaces;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Nrk.FluentCore.Utils;

public static class HttpUtils
{
    public static DownloadSetting DownloadSetting { get; set; } = new()
    {
        EnableLargeFileMultiPartDownload = true,
        FileSizeThreshold = 1024 * 1024 * 3,
        MultiPartsCount = 8,
        MultiThreadsCount = 64
    };

    public static readonly HttpClient HttpClient = new();
    public static readonly MemoryPool<byte> MemoryPool = MemoryPool<byte>.Shared;

    public static HttpResponseMessage HttpPost(string url, string content, Dictionary<string, string> headers, string contentType = "application/json")
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        using var httpContent = new StringContent(content);
        httpContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        if (headers != null)
            foreach (var kvp in headers)
                requestMessage.Headers.Add(kvp.Key, kvp.Value);

        requestMessage.Content = httpContent;

        return HttpClient.Send(requestMessage);
    }

    public static HttpResponseMessage HttpPost(string url, string content, string contentType = "application/json")
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        using var httpContent = new StringContent(content);

        httpContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        requestMessage.Content = httpContent;

        return HttpClient.Send(requestMessage);
    }

    public static HttpResponseMessage HttpGet(string url, Tuple<string, string> authorization = default, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

        if (authorization != null) requestMessage.Headers.Authorization = new AuthenticationHeaderValue(authorization.Item1, authorization.Item2);

        var responseMessage = HttpClient.Send(requestMessage, httpCompletionOption, CancellationToken.None);

        if (responseMessage.StatusCode.Equals(HttpStatusCode.Found))
        {
            string redirectUrl = responseMessage.Headers.Location.AbsoluteUri;

            responseMessage.Dispose();
            GC.Collect();

            return HttpGet(redirectUrl, authorization, httpCompletionOption);
        }

        return responseMessage;
    }

    public static HttpResponseMessage HttpGet(string url, Dictionary<string, string> headers, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

        if (headers != null)
            foreach (var kvp in headers)
                requestMessage.Headers.Add(kvp.Key, kvp.Value);

        var responseMessage = HttpClient.Send(requestMessage, httpCompletionOption, CancellationToken.None);

        if (responseMessage.StatusCode.Equals(HttpStatusCode.Found))
        {
            string redirectUrl = responseMessage.Headers.Location.AbsoluteUri;

            responseMessage.Dispose();
            GC.Collect();

            return HttpGet(redirectUrl, headers, httpCompletionOption);
        }

        return responseMessage;
    }

    public static string ReadAsString(this HttpContent content)
    {
        using var stream = content.ReadAsStream();
        using var streamReader = new StreamReader(stream);

        return streamReader.ReadToEnd();
    }

    public static async Task<DownloadResult> DownloadElementAsync(
        IDownloadElement downloadElement,
        DownloadSetting downloadSetting = default,
        CancellationTokenSource tokenSource = default)
    {
        var settings = downloadSetting ?? DownloadSetting;
        tokenSource ??= new CancellationTokenSource();

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, downloadElement.Url);
        using var responseMessage = await HttpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, tokenSource.Token);

        if (responseMessage.StatusCode.Equals(HttpStatusCode.Found))
        {
            downloadElement.Url = responseMessage.Headers.Location.AbsoluteUri;
            return await DownloadElementAsync(downloadElement, downloadSetting, tokenSource);
        }

        responseMessage.EnsureSuccessStatusCode();

        if (settings.EnableLargeFileMultiPartDownload &&
            responseMessage.Content.Headers.ContentLength.Value > settings.FileSizeThreshold)
            return await TryMultiPartDownloadFileAsync(responseMessage, settings, downloadElement.AbsolutePath, tokenSource)
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                        return new DownloadResult
                        {
                            Exception = task.Exception,
                            IsFaulted = true,
                            DownloadElement = downloadElement
                        };

                    return new DownloadResult
                    {
                        IsFaulted = responseMessage.Content.Headers.ContentLength.Value != task.Result,
                        Exception = responseMessage.Content.Headers.ContentLength.Value != task.Result
                            ? new Exception("文件下载不完整") : null,
                        DownloadElement = downloadElement
                    };
                });
        return await WriteFileFromHttpResponseAsync(responseMessage, downloadElement.AbsolutePath, tokenSource)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                    return new DownloadResult
                    {
                        Exception = task.Exception,
                        IsFaulted = true,
                        DownloadElement = downloadElement
                    };

                return new DownloadResult
                {
                    IsFaulted = responseMessage.Content.Headers.ContentLength.Value != task.Result,
                    Exception = responseMessage.Content.Headers.ContentLength.Value != task.Result
                            ? new Exception("文件下载不完整") : null,
                    DownloadElement = downloadElement
                };
            });
    }

    private async static Task<long> WriteFileFromHttpResponseAsync(
        HttpResponseMessage responseMessage,
        string absolutePath,
        CancellationTokenSource tokenSource)
    {
        var parentFolder = Path.GetDirectoryName(absolutePath);
        if (!Directory.Exists(parentFolder)) Directory.CreateDirectory(parentFolder);

        using var stream = await responseMessage.Content.ReadAsStreamAsync();
        using var fileStream = File.Create(absolutePath);
        using var rentMemory = MemoryPool.Rent(1024);

        long totalReadMemory = 0;
        int readMemory = 0;

        while ((readMemory = await stream.ReadAsync(rentMemory.Memory, tokenSource.Token)) > 0)
        {
            await fileStream.WriteAsync(rentMemory.Memory[..readMemory], tokenSource.Token);
            Interlocked.Add(ref totalReadMemory, readMemory);
        }

        return totalReadMemory;
    }

    private static async Task<long> TryMultiPartDownloadFileAsync(
        HttpResponseMessage responseMessage,
        DownloadSetting downloadSetting,
        string absolutePath,
        CancellationTokenSource tokenSource)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, responseMessage.RequestMessage.RequestUri.AbsoluteUri);
        requestMessage.Headers.Range = new RangeHeaderValue(0, 1);

        using var httpResponse = await HttpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, tokenSource.Token);

        if (!httpResponse.IsSuccessStatusCode)
            return await WriteFileFromHttpResponseAsync(responseMessage, absolutePath, tokenSource);

        var totalSize = responseMessage.Content.Headers.ContentLength.Value;
        var singleSize = totalSize / downloadSetting.MultiPartsCount;

        var rangesList = new List<DownloadRange>();
        var folder = Path.GetDirectoryName(absolutePath);

        while (totalSize > 0)
        {
            bool enough = totalSize - singleSize > 1024 * 10;

            var range = new DownloadRange
            {
                End = totalSize,
                Start = enough ? totalSize - singleSize : 0
            };

            range.TempFileAbsolutePath = Path.Combine(folder, $"{range.Start}-{range.End}-" + Path.GetFileName(absolutePath));
            rangesList.Add(range);

            if (!enough) break;

            totalSize -= singleSize;
        }

        var transformBlock = new TransformBlock<DownloadRange, (HttpResponseMessage, DownloadRange)>(async range =>
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, responseMessage.RequestMessage.RequestUri);
            httpRequest.Headers.Range = new RangeHeaderValue(range.Start, range.End);

            var message = await HttpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, tokenSource.Token);
            return (message, range);
        }, new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = downloadSetting.MultiPartsCount,
            MaxDegreeOfParallelism = downloadSetting.MultiPartsCount,
            CancellationToken = tokenSource.Token
        });

        var actionBlock = new ActionBlock<(HttpResponseMessage, DownloadRange)>
            (async t => await WriteFileFromHttpResponseAsync(t.Item1, t.Item2.TempFileAbsolutePath, tokenSource),
            new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = downloadSetting.MultiPartsCount,
                MaxDegreeOfParallelism = downloadSetting.MultiPartsCount,
                CancellationToken = tokenSource.Token
            });

        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
        var transformManyBlock = new TransformManyBlock<IEnumerable<DownloadRange>, DownloadRange>(chunk => chunk,
            new ExecutionDataflowBlockOptions());

        transformManyBlock.LinkTo(transformBlock, linkOptions);
        transformBlock.LinkTo(actionBlock, linkOptions);

        transformManyBlock.Post(rangesList);
        transformManyBlock.Complete();

        await actionBlock.Completion;

        await using (var outputStream = File.Create(absolutePath))
        {
            foreach (var inputFile in rangesList)
            {
                await using (var inputStream = File.OpenRead(inputFile.TempFileAbsolutePath))
                {
                    outputStream.Seek(inputFile.Start, SeekOrigin.Begin);
                    await inputStream.CopyToAsync(outputStream, tokenSource.Token);
                }

                File.Delete(inputFile.TempFileAbsolutePath);
            }
        }

        return new FileInfo(absolutePath).Length;
    }
}
