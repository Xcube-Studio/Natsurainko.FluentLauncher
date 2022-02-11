using FluentCore.Model;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace FluentCore.Service.Network
{
    public class HttpHelper
    {
        public static int BufferSize { get; set; } = 1024 * 1024;

        public static readonly HttpClient HttpClient;

        static HttpHelper() => HttpClient = new HttpClient();

        public static async Task<HttpResponseMessage> HttpGetAsync(string url, Tuple<string, string> authorization = default, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            if (authorization != null)
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue(authorization.Item1, authorization.Item2);

            var responseMessage = await HttpClient.SendAsync(requestMessage, httpCompletionOption, CancellationToken.None);

            if (responseMessage.StatusCode.Equals(HttpStatusCode.Found))
            {
                string redirectUrl = responseMessage.Headers.Location.AbsoluteUri;

                responseMessage.Dispose();
                GC.Collect();

                return await HttpGetAsync(redirectUrl, authorization, httpCompletionOption);
            }

            return responseMessage;
        }

        public static async Task<HttpResponseMessage> HttpPostAsync(string url, Stream content, string contentType = "application/json")
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            var httpContent = new StreamContent(content);

            httpContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            requestMessage.Content = httpContent;

            var res = await HttpClient.SendAsync(requestMessage);

            content.Dispose();
            httpContent.Dispose();

            return res;
        }

        public static async Task<HttpResponseMessage> HttpPostAsync(string url, string content, string contentType = "application/json")
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            using var httpContent = new StringContent(content);

            httpContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            requestMessage.Content = httpContent;

            var res = await HttpClient.SendAsync(requestMessage);
            return res;
        }

        public static async Task<HttpDownloadResponse> HttpDownloadAsync(string url, string folder, string filename = null)
        {
            FileInfo fileInfo = default;

            try
            {
                using var responseMessage = await HttpGetAsync(url, default, HttpCompletionOption.ResponseHeadersRead);

                if (!responseMessage.IsSuccessStatusCode)
                    return new HttpDownloadResponse
                    {
                        FileInfo = null,
                        HttpStatusCode = responseMessage.StatusCode,
                        Message = $"{responseMessage.ReasonPhrase}[{url}]"
                    };

                if (responseMessage.Content.Headers != null && responseMessage.Content.Headers.ContentDisposition != null)
                    fileInfo = new FileInfo(Path.Combine(folder, responseMessage.Content.Headers.ContentDisposition.FileName.Trim('\"')));
                else fileInfo = new FileInfo(Path.Combine(folder, Path.GetFileName(responseMessage.RequestMessage.RequestUri.AbsoluteUri)));

                if (filename != null)
                    fileInfo = new FileInfo(fileInfo.FullName.Replace(fileInfo.Name, filename));

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                await using var fileStream = new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                await using var stream = await responseMessage.Content.ReadAsStreamAsync();

                byte[] bytes = new byte[BufferSize];
                int read = await stream.ReadAsync(bytes.AsMemory(0, BufferSize));

                while (read > 0)
                {
                    await fileStream.WriteAsync(bytes.AsMemory(0, read));
                    read = await stream.ReadAsync(bytes.AsMemory(0, BufferSize));
                }

                fileStream.Flush();
                fileStream.Close();
                stream.Close();

                GC.Collect();

                return new HttpDownloadResponse
                {
                    FileInfo = fileInfo,
                    HttpStatusCode = responseMessage.StatusCode,
                    Message = $"{responseMessage.ReasonPhrase}[{url}]"
                };
            }
            catch (HttpRequestException e)
            {
                GC.Collect();

                return new HttpDownloadResponse
                {
                    FileInfo = fileInfo,
                    HttpStatusCode = e.StatusCode.Value,
                    Message = $"{e.Message}[{url}]"
                };
            }
        }

        public static async Task<HttpDownloadResponse> HttpDownloadAsync(HttpDownloadRequest request, string filename = null) => await HttpDownloadAsync(request.Url, request.Directory.FullName, filename);

        public static void SetTimeout(int milliseconds) => HttpClient.Timeout = new TimeSpan(0, 0, 0, 0, milliseconds);
    }
}
